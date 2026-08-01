# Warehouse Management System (WMS) — Proje Rehberi

Bu dosya, projenin mimari kararlarını, teknoloji yığınını, naming standardını, domain/iş kurallarını ve geliştirme döngüsünü tanımlar. Yeni kod yazılırken bu dosyadaki kararlara **uyulmalıdır**; sapma gerekiyorsa önce bu dosya güncellenir.

## Kapsam

MVP kapsamı: tek şirket, çoklu depo destekli bir **Depo Yönetim Sistemi**. Ürün/kategori tanımlama, depo tanımlama, mal kabul (inbound), sevkiyat/mal çıkışı (outbound), depolar arası transfer, stok sayımı ve sayım düzeltme, temel raporlama. Raf/lokasyon (bin) seviyesi stok takibi ve çok şirketli (multi-tenant) yapı MVP dışıdır.

---

## 1. Mimari

- **Clean Architecture**: Domain → Application → Infrastructure → Api (Presentation). Bağımlılıklar her zaman içe doğru; Domain katmanı hiçbir dış katmana bağımlı değildir.
- **Modüler Monolith**: Her iş modülü kendi Domain/Application/Infrastructure projelerine ve kendi PostgreSQL şemasına sahiptir. Modüller birbirine **doğrudan proje referansı vermez**; yalnızca MediatR (command/query/notification) üzerinden haberleşir. Her modülün `Infrastructure` katmanındaki `Add{Module}Module(IServiceCollection, IConfiguration)` extension'ı, o modülün kendi `Application` assembly'sinden MediatR handler'larını ve FluentValidation validator'larını kaydeder (bkz. `{Module}ApplicationAssemblyMarker` sınıfları); `WMS.Api/Program.cs` sadece bu extension'ları çağırır.
- **CQRS**:
  - **Yazma (Command)**: EF Core + LINQ, domain aggregate'leri üzerinden, `I{Aggregate}WriteRepository` (EF Core) kullanılır.
  - **Okuma (Query)**: Dapper + ham SQL, doğrudan DTO/read-model döndürür, `I{Aggregate}ReadRepository` (Dapper) kullanılır. Karmaşık raporlama sorguları burada yazılır.
  - Bir command handler asla Dapper okuma repository'si kullanmaz; bir query handler asla EF Core write repository'si kullanmaz.
- **Event-Driven (in-process, RabbitMQ YOK), transactional outbox ile**: MediatR `INotification` ile domain event'ler yayınlanır, ama artık bellek içi anlık dispatch değil, **outbox pattern** üzerinden: `OutboxWritingInterceptor` (`WMS.BuildingBlocks.Infrastructure.Outbox`) domain event'leri, aggregate'in kendi değişikliğiyle **aynı `SaveChangesAsync` transaction'ında** `outbox_messages` tablosuna yazar (pre-commit `SavingChangesAsync` hook'u) — bu sayede process commit ile dispatch arasında çökerse event asla sessizce kaybolmaz. Her event ÜRETEN modül (Inbound, Outbound, Transfer, StockCount — Inventory/Catalog/Identity event üretmiyor) kendi şemasında bir `outbox_messages` tablosuna sahiptir. Her üretici modülün kendi `Add{Module}Module`'unda register edilen generic `OutboxProcessor<TDbContext>` (`BackgroundService`, ilk hosted service — 5sn polling, batch 20, `RetryCount < 10`) bu satırları okuyup MediatR üzerinden **en az bir kez** (at-least-once) teslim eder — mevcut domain event handler'lar (`{Aggregate}{PastTenseVerb}DomainEventHandler`) değişmeden aynı kalır, sadece tetiklenme yeri (interceptor → relay) değişir.
- **Idempotency guard (Inventory)**: At-least-once teslimat, aynı mesajın iki kez işlenme riski taşır (relay bir mesajı işleyip "processed" işaretlemeden çökerse); stok artırma/azaltma doğası gereği idempotent olmadığı için (aynı komut iki kez = yanlış miktar) Inventory modülü `ProcessedDomainEvent` adlı küçük bir ledger tablosu tutar, `(SourceEventId, LineNumber)` composite key ile — `SourceEventId` üretici modülün outbox mesaj Id'si, `LineNumber` bir event'in (örn. çok satırlı bir Mal Kabul) içindeki 0-tabanlı satır sırası. `IncreaseStockCommand`/`DecreaseStockCommand` bu iki alanı zorunlu parametre olarak alır, handler önce `ExistsAsync` kontrolü yapar (varsa no-op `Result.Success()`), yoksa stok değişikliği + ledger satırı **aynı `SaveChangesAsync` çağrısında** commit edilir (atomiklik buradan gelir). Inventory tüm event akışlarının tek tüketicisi olduğu için tek bir ledger tablosu yeterli.
- **Modüller arası çağrı kalıbı**: "Modüller birbirine doğrudan proje referansı vermez" kuralı Domain/Infrastructure için geçerlidir; bir modülün `Application` katmanı, **başka bir modülün `Application` projesine referans verip onun Command/Query tiplerini `ISender` ile çağırabilir** — bu, modülün genel API'sidir (Inbound → Inventory'nin `IncreaseStockCommand`'ı ve Catalog'un `GetProductByIdQuery`'si için yaptığı gibi, bkz. `WMS.Modules.Inbound.Application` → `WMS.Modules.Inventory.Application`/`WMS.Modules.Catalog.Application` proje referansları). Domain event'e tepki veren handler her zaman event'i **üreten modülde** yaşar (örn. `GoodsReceiptApprovedDomainEventHandler` Inbound'da yaşar ve Inventory'nin komutunu çağırır) — asla tüketen modülde değil; bu, bağımlılık yönünü tek taraflı tutar ve yeni bir üretici modül eklendiğinde Inventory'nin değişmesini gerektirmez. Bu şekilde tetiklenen komutlar (örn. stok artırma) event üreten modülün **kendi transaction'ından ayrı**, kendi DbContext'i içinde ayrı bir transaction'da çalışır (aynı fiziksel veritabanı olsa da farklı DbContext = farklı transaction); bu yüzden relay tarafından çağrılan domain event handler'lar, çağırdıkları komut `Result.Failure` dönerse **exception fırlatmaz** (zaten commit olmuş üretici işlemi yanlışlıkla hataya çevirmez), bunun yerine hatayı `ILogger` ile loglar — outbox sadece "process çöktü, event kayboldu" senaryosunu kapatır, "downstream iş kuralı reddetti" davranışını değiştirmez, bu hâlâ MVP için kabul edilen bir sınırdır.
- **Seeding/bootstrap kodu asenkron event akışına dikkat etmeli**: `DemoDataSeeder` gibi, bir komutun tetiklediği cross-module event'in sonucuna (örn. bir Mal Kabul onayının Inventory'e yansımasına) hemen ardından bağımlı bir işlem yapan kod, artık bu güncellemenin `OutboxProcessor`'ın bir sonraki poll döngüsünde (asenkron) geleceğini varsaymalı — `DemoDataSeeder` bunu ilgili sorguyu (örn. `GetStockItemsQuery`) kısa aralıklarla polling ile bekleyerek çözüyor (bkz. `WMS.Api/Seeding/DemoDataSeeder.cs`). Ayrıca `Program.cs` seed adımını artık `app.Run()`'dan önce değil, `app.StartAsync()` sonrası (hosted service'ler, yani `OutboxProcessor`'lar başladıktan sonra) çalıştırıyor — aksi halde relay hiç çalışmadan seed kodu asenkron güncellemeyi asla göremezdi.
- **Repository Pattern**: Her aggregate için ayrı okuma ve yazma arayüzü (`I{Aggregate}ReadRepository`, `I{Aggregate}WriteRepository`). Bkz. Naming Standardı.
- **Veritabanı**: Tek PostgreSQL instance, modül başına ayrı **schema**: `identity`, `catalog`, `inventory`, `inbound`, `outbound`, `transfer`, `stockcount`. Yazma tarafı şema sınırını ihlal etmez. Dapper okuma/raporlama tarafı, raporlama ihtiyacı için şemalar arası join yapabilir (pragmatik istisna).
- **Kimlik Doğrulama**: JWT Bearer (access + refresh token). **Rol bazlı yetkilendirme** (`[Authorize(Roles = "...")]` / policy-based).

### Dapper Kuralları

- **Read-model DTO'larda tarih/saat alanları için `DateTimeOffset` değil `DateTime` kullanılır.** EF Core (yazma tarafı) `DateTimeOffset` property'lerini `timestamptz` kolonuna sorunsuz eşler, ama Dapper'ın kullandığı ham Npgsql ADO.NET okuma yolu `timestamptz` kolonlarını varsayılan olarak `DateTime` (UTC) döndürür — DTO/record'da `DateTimeOffset` kullanılırsa Dapper record constructor'ını eşleştiremez ve `InvalidOperationException: A parameterless default constructor or one matching signature ... is required` hatasıyla materialize işlemi başarısız olur (Inbound modülünde `GoodsReceiptDto.CreatedAtUtc`/`ApprovedAtUtc` ile karşılaşıldı). Kural: Dapper ile doldurulan tüm DTO'larda tarih alanları `DateTime`/`DateTime?` olacak (değer zaten UTC).

### EF Core Kuralları

- **Tüm entity Id'leri client-side üretilir** (`Guid.CreateVersion7()`, bkz. `BaseEntity`), veritabanı tarafından değil. Bu nedenle her entity konfigürasyonunda **`builder.Property(x => x.Id).ValueGeneratedNever();` zorunludur**. Aksi halde EF Core, zaten set edilmiş (default olmayan) bir Guid değeriyle collection-fixup üzerinden (örn. `aggregate.Children.Add(yeni)`) eklenen yeni bir child entity'yi "Modified" sanıp UPDATE atmaya çalışır, bu da 0 satır etkilenen `DbUpdateConcurrencyException` ile sonuçlanır (Identity modülünde `RefreshToken` eklerken tam olarak bu hatayla karşılaşıldı). Her yeni entity konfigürasyonunda bu satırı eklemeyi unutma.
- **Naming convention**: `EFCore.NamingConventions` paketi + `options.UseSnakeCaseNamingConvention()` ile tüm tablo/kolon adları otomatik snake_case (Postgres konvansiyonu). Manuel `HasColumnName` gerekmez.
- **Optimistic Concurrency**: Aynı satırda **eşzamanlı yarış durumu olabilecek** her yerde optimistic concurrency token kullanılacak — özellikle stok miktarını değiştiren tüm akışlarda (Inbound onayı, Outbound onayı, Transfer gönder/teslim al, StockCount düzeltme onayı → hepsi `Inventory` modülündeki `StockItem.Quantity`'yi günceller). Postgres'in `xmin` sistem kolonu concurrency token olarak kullanılacak — entity konfigürasyonunda `builder.Property<uint>("xmin").IsRowVersion();` (Npgsql EF Core sağlayıcısında ayrı bir `.UseXminAsConcurrencyToken()` extension'ı **yok**, bu shadow-property kalıbı doğru API'dir — Inventory modülü implementasyonunda derleme hatasıyla doğrulandı). Infrastructure katmanındaki repository, `DbUpdateConcurrencyException`'ı yakalayıp EF Core'a bağımlı olmayan `WMS.SharedKernel.ConcurrencyConflictException`'a çevirir; Application katmanındaki handler bunu yakalayıp `Error.Conflict(...)` içeren bir `Result` döner — asla kullanıcıya çıplak 500 olarak yansımaz. Bu kalıp Faz 4'te (Inventory modülü, `StockItem`) uygulandı; salt referans/seed veri (Role gibi) veya tek kullanıcı tarafından değiştirilen kayıtlarda gerekmez.

### Backend Test Kuralları

- **Test proje yapısı**: `tests/WMS.Modules.*.UnitTests` (xUnit + FluentAssertions + NSubstitute, handler'lar mock repository/`ISender` ile test edilir — `ISender.Send(Arg.Any<GetXByIdQuery>(), ...)` NSubstitute ile doğrudan mock'lanabiliyor, MediatR'a bağımlı gerçek bir pipeline kurmaya gerek yok), `tests/WMS.Modules.*.IntegrationTests` (Testcontainers.PostgreSql ile gerçek bir Postgres'e karşı EF write + Dapper read repository'leri test edilir — 7 modülün tamamı yerine temsili 3 modül seçildi: Inventory [xmin concurrency conflict — sistemin en kritik altyapı parçası], Catalog [en basit EF yaz + Dapper oku roundtrip deseni], StockCount [iki ayrı aggregate root ilişkisi]), `tests/WMS.Api.FunctionalTests` (WebApplicationFactory + Testcontainers ile gerçek HTTP istekleri, tüm modüller gerçek DI ile ayağa kalkar).
- **`WebApplicationFactory<Program>` + Testcontainers kullanırken bağlantı dizesini override etme tuzağı**: `ConfigureWebHost(builder => builder.ConfigureAppConfiguration(...))` ile `ConnectionStrings:Default`'ı override etmek bu projede **işe yaramıyor** — çünkü `Program.cs` bağlantı dizesini `builder.Build()` çağrılmadan **önce** bir local değişkene okuyup modül DI extension'larına (`AddCatalogModule(configuration)` vb.) geçiyor, oysa `WebApplicationFactory`'nin host-interception mekanizması `ConfigureAppConfiguration` callback'lerini ancak `Build()` anında uyguluyor — yani override, Program.cs'in kendi kodu çalıştıktan **sonra** gelip bir işe yaramıyor (ilk denemede sessizce geliştiricinin gerçek lokal veritabanına bağlanıp orada zaten var olan veriyle çakışan bir 409 üretti, `WMS.Api.FunctionalTests`'te tam olarak bu şekilde keşfedildi). **Çözüm**: `Environment.SetEnvironmentVariable("ConnectionStrings__Default", ...)` ile gerçek bir ortam değişkeni set et (factory'nin `IAsyncLifetime.InitializeAsync()`'inde, container başladıktan hemen sonra) — ortam değişkenleri `WebApplication.CreateBuilder()`'ın kendi varsayılan konfigürasyon kaynaklarının bir parçası olarak Program.cs'in kendi kodundan **önce** okunuyor, bu yüzden zamanlaması doğru.
- **Fonksiyonel test sınıfları arasında `IClassFixture<CustomWebApplicationFactory>` yerine paylaşılan bir `ICollectionFixture` kullan**: Her test sınıfı kendi `WebApplicationFactory`'sini (dolayısıyla kendi Postgres container'ını) ayrı ayrı oluşturursa, xUnit farklı sınıfları paralel çalıştırdığında `WebApplicationFactory`'nin host-interception mekanizması (aynı process içinde eşzamanlı birden fazla host inşa etme) çakışıp rastgele "The entry point exited without ever building an IHost" hatası veriyor. Çözüm: tek bir `[CollectionDefinition]` + `ICollectionFixture<CustomWebApplicationFactory>` ile **tüm** fonksiyonel test sınıfları arasında **tek** bir factory/container paylaşılır (xUnit aynı collection'daki sınıfları asla birbiriyle paralel çalıştırmıyor) — hem bu çakışmayı çözüyor hem de container başlatma maliyetini tek seferliğe indiriyor.

### Backend Proje Yapısı

```
backend/
  src/
    WMS.Api/                          # Composition root: controllers, middleware, DI, appsettings
    WMS.SharedKernel/                 # BaseEntity, IDomainEvent, Result<T>, Error, Guard
    BuildingBlocks/
      WMS.BuildingBlocks.Application/     # ICommand/IQuery/handler abstractions, MediatR pipeline behaviors
      WMS.BuildingBlocks.Infrastructure/  # EF Core SaveChanges interceptor that dispatches domain events via MediatR
    Modules/
      Identity/
        WMS.Modules.Identity.Domain
        WMS.Modules.Identity.Application
        WMS.Modules.Identity.Infrastructure
      Catalog/
      Inventory/
      Inbound/
      Outbound/
      Transfer/
      StockCount/
  tests/
    WMS.Modules.*.UnitTests
    WMS.Modules.*.IntegrationTests    # Testcontainers + PostgreSQL
    WMS.Api.FunctionalTests
```

Her modül aynı üçlü yapıya sahiptir: `Domain` (entity, value object, domain event, enum), `Application` (Command/Query/Handler/DTO/Validator, repository arayüzleri), `Infrastructure` (EF Core DbContext + migration, EF write-repository implementasyonu, Dapper read-repository implementasyonu).

**C# namespace/tür adı çakışması**: Bir modülün namespace'i (`WMS.Modules.{Module}`) ile o modüldeki bir aggregate root'un adı birebir aynı olursa (StockCount modülünde olduğu gibi: `WMS.Modules.StockCount` namespace'i ve `WMS.Modules.StockCount.Domain.StockCount` sınıfı), `using WMS.Modules.StockCount.Domain;` + çıplak `StockCount` kullanımı CS0118 hatası verir — çünkü C#'ın isim çözümleme sırası, dıştaki ad alanlarında (burada `WMS.Modules` seviyesinde) aynı adlı bir alt-namespace bulduğunda, henüz `using` ile içe aktarılan türe bakmadan orada durur. Çözüm: o sınıfa referans veren her dosyada `using StockCountAggregate = WMS.Modules.StockCount.Domain.StockCount;` gibi bir alias kullan. Yeni bir modül eklerken modül adını doğrudan bir aggregate root adı olarak seçme; seçmek zorundaysan bu alias kalıbını uygula.

---

## 2. Naming Standardı — TEK STANDART, HER YERDE AYNI

| Tür | Kalıp | Örnek |
|---|---|---|
| Command | `{Verb}{Aggregate}Command` | `CreateProductCommand` |
| Command Handler | `{Verb}{Aggregate}CommandHandler` | `CreateProductCommandHandler` |
| Command Validator | `{Verb}{Aggregate}CommandValidator` | `CreateProductCommandValidator` |
| Query | `Get{Aggregate}{Suffix}Query` | `GetProductByIdQuery`, `GetProductsQuery` |
| Query Handler | `Get{Aggregate}{Suffix}QueryHandler` | `GetProductByIdQueryHandler` |
| DTO | `{Aggregate}Dto` | `ProductDto` |
| Domain Event | `{Aggregate}{PastTenseVerb}DomainEvent` | `StockCountCompletedDomainEvent` |
| Domain Event Handler | `{Aggregate}{PastTenseVerb}DomainEventHandler` | `StockCountCompletedDomainEventHandler` |
| Read Repository | `I{Aggregate}ReadRepository` / `{Aggregate}ReadRepository` | `IProductReadRepository` |
| Write Repository | `I{Aggregate}WriteRepository` / `{Aggregate}WriteRepository` | `IProductWriteRepository` |
| Controller | `{AggregatePlural}Controller` | `ProductsController` |

**Kural**: Çıplak `XxxHandler` kullanılmaz — her zaman Command/Query/DomainEvent son eki dahil edilir. Bu tabloya uymayan isimlendirme code review'da reddedilir.

---

## 3. Teknoloji Yığını

### Backend
- .NET 10 LTS, ASP.NET Core Web API
- MediatR (CQRS + in-process event bus)
- FluentValidation (command/query validation, MediatR pipeline behavior olarak)
- EF Core (yazma tarafı) + Npgsql provider
- Dapper (okuma/raporlama tarafı) + Npgsql
- PostgreSQL (modül başına schema)
- JWT Bearer authentication, role-based authorization
- Serilog (logging)
- Test: xUnit, FluentAssertions, Moq/NSubstitute, Testcontainers.PostgreSql, WebApplicationFactory

### Frontend
- React + Vite + TypeScript (Next.js **kullanılmıyor**)
- TanStack Query: **server state** (API çağrıları, cache, invalidation)
- Zustand: **client/UI state** (seçili depo, filtre durumları, tema vb.)
- Axios: merkezi instance, interceptor ile JWT header + 401 refresh/redirect
- shadcn/ui + Tailwind CSS: dark/light tema desteği (CSS variables + `prefers-color-scheme` + manuel toggle, localStorage)
- Feature-based klasörleme
- Test: Vitest + React Testing Library

### Frontend Proje Yapısı

```
frontend/
  src/
    app/
      App.tsx             # ThemeProvider + QueryClientProvider + RouterProvider + Toaster
      router.tsx          # createBrowserRouter — ProtectedRoute > AppLayout > sayfalar
      layout/AppLayout.tsx # sol nav + header (kullanıcı adı, tema, çıkış)
      providers/          # ThemeProvider
      routes/             # ProtectedRoute, RoleGuard — sayfa değil, route yardımcıları
    features/
      auth/
      products/            # Product + Category + UnitOfMeasure (Catalog modülüyle birebir) + ProductLookupDialog
      warehouses/
      inbound/             # Mal Kabul (GoodsReceipt)
      outbound/            # Sevkiyat (GoodsIssue)
      transfer/            # Depolar arası transfer (StockTransfer)
      stock-count/
      reports/
      # her feature: api/ (query/mutation hook'ları), types.ts, {Feature}Page.tsx (+ .test.tsx), New{Entity}Page.tsx (çok satırlı formlar için)
    components/ui/         # shadcn bileşenleri (Base UI tabanlı — bkz. Frontend Kuralları)
    lib/                    # axios, query-client, errors, dates, pagination, utils
    hooks/                  # useDebouncedValue vb.
    types/
```

Naming: API hook'ları `use{Verb}{Resource}` (örn. `useCreateProduct`, `useProducts`), Zustand store'ları `use{Feature}Store`. Liste+CRUD sayfası `{AggregatePlural}Page` (örn. `ProductsPage`), çok satırlı oluşturma formu ayrı bir sayfa olarak `New{Aggregate}Page` (dialog değil — bkz. Frontend Kuralları, "İki/çok adımlı workflow ekranları").

### Frontend Kuralları

- **State ayrımı sıkı uygulanır**: Backend'den gelen HER ŞEY (liste verisi, kullanıcı profili, roller — hiçbir istisna yok) TanStack Query'de yaşar; Zustand sadece gerçekten client-only olan şeyler içindir (`accessToken`/`refreshToken` gibi React ağacı dışından — örn. axios interceptor'dan — okunması gereken durum, tema, filtre state'i). Faz10'da kullanıcı profili yanlışlıkla Zustand store'a konulmuş, Faz11'de `useCurrentUser()` (TanStack Query) hook'una taşındı — bu hata tekrarlanmamalı.
- **`lib/errors.ts`**: `getApiErrorMessage(error, fallback?)` — backend'in `ProblemDetails.title`'ını bilinen bir `KNOWN_ERROR_MESSAGES` sözlüğünde Türkçe mesaja çevirir, bilinmeyen bir kod için backend'in İngilizce `detail`'ini **asla** doğrudan göstermez (genel Türkçe fallback döner). Kullanıcının normal kullanımda tetikleyebileceği her hata kodu (409/400 InUse, InsufficientStock, SameWarehouse gibi) için sözlüğe bir satır eklenir; UI'ın zaten engellediği "olmaması gereken" hatalar (NotFound/NotDraft gibi) için gerek yok.
- **`lib/dates.ts`**: `formatUtcDateTime(value)` — Dapper okuma tarafının döndürdüğü `DateTime` alanları (bkz. Dapper Kuralları) JSON'da 'Z' son eki olmadan UTC gelir; ham `new Date(value)` bunu yerel saat sanır. Backend'den gelen her tarih/saat alanı bu yardımcıyla gösterilir.
- **`lib/pagination.ts`**: `PagedResult<T> = {items, totalCount, page, pageSize}` — backend'in `PagedResult<T>`'ına birebir karşılık gelir. Potansiyel olarak çok büyüyebilecek listeler (yüz binlerce kayıt) sayfalanır; doğası gereği küçük kalacak referans listeleri (Depo/Kategori/Birim gibi) sayfalanmaz.
- **shadcn "base" (Base UI) bileşen kütüphanesi — iki zorunlu nokta**: (1) Radix'teki `asChild` yerine `render={<Comp/>}` prop deseni kullanılır. (2) `Select.Root`'un seçili değerin etiketini gösterebilmesi için `items` prop'u (value→label map'i) zorunludur.
- **Test yaklaşımı (MVP boyunca)**: Her fazda kapsamlı değil, temsili birkaç "smoke test" yazılır — kapsamlı frontend testleri Faz14'e bırakılmıştır. Base UI `Select` içeren etkileşim testleri jsdom'da kırılgan olabilir; mümkün olduğunda Select yerine `Input`/`Table` kullanan bileşenler tercih edilip test edilir, Select-ağırlıklı formlar gerçek tarayıcıda manuel doğrulanır.
- **Modül/feature ve iş akışı dokümantasyonu `docs/workflows/`'ta tutulur** (bkz. §9) — CLAUDE.md sadece proje geneli mimari/kural/konvansiyonları içerir, tek bir modülün/iş akışının API sözleşmesi, rol modeli veya UI kararları burada anlatılmaz.

---

## 4. Roller ve Yetkiler (MVP)

| Rol | Yetki özeti |
|---|---|
| **Admin** | Sistem geneli tam yetki: kullanıcı/rol yönetimi, tüm depolar, tüm modüller |
| **DepoMüdürü** | Kendi deposunda tam yetki: transfer onayı, sayım düzeltme onayı, mal kabul/sevkiyat onayı, raporlar |
| **DepoSorumlusu** | Günlük operasyon yönetimi: sayım oturumu açma/kapama, mal kabul & sevkiyat onaylama |
| **DepoPersoneli** | Veri girişi: sayım sayma (satır girişi), mal kabul/sevkiyat satırı girme (onay gerektirmez) |

---

## 5. Entity / Tablo Taslağı (MVP)

- **Identity şeması**: `User`, `Role`, `UserRole`, `RefreshToken`. **Not**: MVP'de ayrı bir `Permission` entity'si yok — yetkilendirme tamamen rol bazlı (`[Authorize(Roles=...)]`), 4 rol sabit seed data (`RoleNames`/`RoleIds`/`RoleCatalog` sınıfları). `UserWarehouse` (kullanıcı-depo ataması) Faz 4'e (Inventory modülü, `Warehouse` entity'si var olduğunda) ertelendi.
- **Catalog şeması**: `Product` (SKU, ad, birim, min stok), `Category`, `UnitOfMeasure`
- **Inventory şeması**: `Warehouse`, `StockItem` (WarehouseId + ProductId + Quantity), `StockMovement` (tüm stok değişimlerinin audit ledger'ı)
- **Inbound şeması**: `GoodsReceipt` (Depo, Tarih, Durum, OluşturanKullanıcı), `GoodsReceiptLine` (Ürün, Miktar)
- **Outbound şeması**: `GoodsIssue` (Depo, Tarih, Durum, Hedef/Açıklama), `GoodsIssueLine` (Ürün, Miktar)
- **Transfer şeması**: `StockTransfer` (KaynakDepo, HedefDepo, Durum: Draft/Shipped/Received/Cancelled), `StockTransferLine` (Ürün, Miktar)
- **StockCount şeması**: `StockCount` (Depo, Durum: Draft/InProgress/Completed, OluşturanKullanıcı), `StockCountLine` (Ürün, SistemMiktarı, SayılanMiktar, Fark), `StockCountAdjustment` (StockCountLine referansı, Fark miktarı, OnaylayanKullanıcı, Durum: Pending/Approved/Rejected)

---

## 6. İş Akışları (MVP)

1. **Giriş & Yetkilendirme**: Login → JWT (access + refresh) → role claim'e göre UI/endpoint erişimi.
2. **Ürün/Kategori/Depo Tanımlama**: Basit CRUD (Admin/DepoMüdürü).
3. **Mal Kabul (Inbound)**: DepoPersoneli satırları girer → DepoSorumlusu onaylar → `GoodsReceiptApprovedDomainEvent` → Inventory modülü `StockItem` miktarını artırır + `StockMovement` kaydı.
4. **Sevkiyat / Mal Çıkışı (Outbound)**: DepoPersoneli satırları girer (yeterli stok kontrolü) → DepoSorumlusu onaylar → `GoodsIssueApprovedDomainEvent` → Inventory stok azaltır + `StockMovement` kaydı.
5. **Depolar Arası Transfer**: DepoPersoneli/Sorumlusu transfer oluşturur (Kaynak→Hedef) → Kaynak depo onaylar ve gönderir (`StockTransferShippedDomainEvent`: kaynak stok düşer) → Hedef depo teslim alır (`StockTransferReceivedDomainEvent`: hedef stok artar). İki adımlı, yolda kayıp/fark senaryosuna açık.
6. **Sayım (Stock Count)**: DepoSorumlusu sayım oturumu açar → DepoPersoneli ürün bazında sayılan miktarı girer → sistem miktarı ile farkı hesaplar.
7. **Sayım Düzeltme (Adjustment)**: Farkı olan satırlar için düzeltme kaydı oluşur → DepoMüdürü onaylar → `StockCountAdjustmentApprovedDomainEvent` → Inventory stok düzeltilir + `StockMovement` kaydı (audit).
8. **Raporlama**: Depo bazlı güncel stok, stok hareket geçmişi (ledger), sayım fark raporu — tamamı Dapper ile.

---

## 7. Docker Compose

`docker-compose.yml` (repo kökü) servisleri:
- `postgres`: PostgreSQL 17, kalıcı volume, healthcheck
- `backend`: `backend/Dockerfile` (multi-stage .NET 10 SDK build → runtime image)
- `frontend`: `frontend/Dockerfile` (multi-stage: `npm run build` → nginx statik servis)
- Ortak `.env` (DB adı/kullanıcı/şifre, JWT secret, portlar)

---

## 8. Geliştirme Döngüsü

Her görev için: **Planla → Implement et → Testleri güncelle/yaz → Dökümanları güncelle/yaz → Commit mesajı yaz ve review için sun → Onay al → Commit et → `TASKS.md`'de işaretle.**

Kurallar:
- **Commit mesajları her zaman İngilizce yazılır** (proje dili Türkçe olsa da).
- Commit mesajlarına `Co-Authored-By` gibi Claude/Anthropic referansı eklenmez.
- Yeni bir command/query/handler/DTO yazılırken **Bölüm 2'deki naming standardına** birebir uyulur.
- Bir modül başka bir modülün Domain/Application/Infrastructure projesine referans vermez; sadece MediatR contract'ları (command/query/notification tipleri) paylaşılabilir.
- Command handler'lar EF Core, query handler'lar Dapper kullanır; bu ayrım asla karıştırılmaz.
- Domain event'ler `SaveChangesAsync` sonrası dispatch edilir.
- Testler: yeni her modül için en az unit test (handler) + entegre test (repository/DB) yazılır.
- Bir iş akışı (modül) baştan sona (backend+frontend) bittiğinde veya önemli ölçüde değiştiğinde **"Dökümanları güncelle/yaz"** adımı `docs/workflows/{isakisi}.md`'yi de kapsar (bkz. §9) — sadece `TASKS.md` işaretlemek yeterli değildir.

---

## 9. Modül / İş Akışı Dokümantasyonu

CLAUDE.md **sadece** proje geneli mimari kararları, naming standardını ve tekrar eden konvansiyonları içerir — tek bir modülün veya iş akışının API sözleşmesi, rol modeli, ekran/bileşen kararları ya da bilinen kısıtları burada **anlatılmaz**, aksi halde bu dosya çok şişer ve genel kural aramak zorlaşır.

Her iş akışı (Mal Kabul, Sevkiyat, Transfer, Sayım, ileride eklenecekler) için `docs/workflows/{isakisi-kebab-case}.md` dosyası açılır ve şunları içerir:
- **Özet**: kim, ne zaman, neden kullanır.
- **Durum makinesi**: durumlar ve hangi command/action hangi geçişi tetikler.
- **Roller**: oluşturma/onay/gönderim/teslim gibi her eylem için hangi rollerin yetkili olduğu.
- **Backend API**: route tablosu (method+path), request/response DTO şekli, hata kodları (ProblemDetails `title` + HTTP status + tetikleyen koşul).
- **Frontend**: sayfa/bileşen dosya yolları, önemli tasarım kararları ve neden öyle yapıldığı.
- **Bilinen kısıtlar / gotcha'lar**: backend'in davranışından kaynaklanan ve frontend'de telafi edilen durumlar (örn. bir kontrolün satır bazında bağımsız yapılması).
- **Doğrulama**: uçtan uca test edilmiş senaryolar.

Basit CRUD referans verileri (Ürün/Kategori/Birim/Depo gibi durum makinesi olmayan ekranlar) için ayrı bir workflow dokümanı zorunlu değildir — bunlar CLAUDE.md §5'teki entity taslağı ve kod ile yeterince açıktır; büyürlerse yine de `docs/workflows/` altına eklenebilir.
