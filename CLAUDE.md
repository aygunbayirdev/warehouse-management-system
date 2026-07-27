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
- **Event-Driven (in-process, RabbitMQ YOK)**: MediatR `INotification` ile domain event'ler yayınlanır. Event'ler `SaveChangesAsync` **sonrası** (transaction commit sonrası) dispatch edilir. Modüller arası reaksiyonlar da aynı mekanizma ile sağlanır — bu, modüller arası gevşek bağlılığı garanti eder.
- **Modüller arası çağrı kalıbı**: "Modüller birbirine doğrudan proje referansı vermez" kuralı Domain/Infrastructure için geçerlidir; bir modülün `Application` katmanı, **başka bir modülün `Application` projesine referans verip onun Command/Query tiplerini `ISender` ile çağırabilir** — bu, modülün genel API'sidir (Inbound → Inventory'nin `IncreaseStockCommand`'ı ve Catalog'un `GetProductByIdQuery`'si için yaptığı gibi, bkz. `WMS.Modules.Inbound.Application` → `WMS.Modules.Inventory.Application`/`WMS.Modules.Catalog.Application` proje referansları). Domain event'e tepki veren handler her zaman event'i **üreten modülde** yaşar (örn. `GoodsReceiptApprovedDomainEventHandler` Inbound'da yaşar ve Inventory'nin komutunu çağırır) — asla tüketen modülde değil; bu, bağımlılık yönünü tek taraflı tutar ve yeni bir üretici modül eklendiğinde Inventory'nin değişmesini gerektirmez. Bu şekilde tetiklenen komutlar (örn. stok artırma) event üreten modülün **kendi transaction'ından ayrı**, kendi DbContext'i içinde ayrı bir transaction'da çalışır (aynı fiziksel veritabanı olsa da farklı DbContext = farklı transaction); bu yüzden `SaveChangesAsync` sonrası çalışan domain event handler'lar, çağırdıkları komut başarısız olursa **exception fırlatmaz** (zaten commit olmuş üretici işlemi yanlışlıkla 500'e çevirip HTTP çağrısını başarısız gösterir), bunun yerine hatayı `ILogger` ile loglar. Bu MVP için kabul edilen bir eventual-consistency riskidir (outbox/saga pattern'i kapsam dışı); nadir bir hata durumunda manuel mutabakat gerekebilir.
- **Repository Pattern**: Her aggregate için ayrı okuma ve yazma arayüzü (`I{Aggregate}ReadRepository`, `I{Aggregate}WriteRepository`). Bkz. Naming Standardı.
- **Veritabanı**: Tek PostgreSQL instance, modül başına ayrı **schema**: `identity`, `catalog`, `inventory`, `inbound`, `outbound`, `transfer`, `stockcount`. Yazma tarafı şema sınırını ihlal etmez. Dapper okuma/raporlama tarafı, raporlama ihtiyacı için şemalar arası join yapabilir (pragmatik istisna).
- **Kimlik Doğrulama**: JWT Bearer (access + refresh token). **Rol bazlı yetkilendirme** (`[Authorize(Roles = "...")]` / policy-based).

### Dapper Kuralları

- **Read-model DTO'larda tarih/saat alanları için `DateTimeOffset` değil `DateTime` kullanılır.** EF Core (yazma tarafı) `DateTimeOffset` property'lerini `timestamptz` kolonuna sorunsuz eşler, ama Dapper'ın kullandığı ham Npgsql ADO.NET okuma yolu `timestamptz` kolonlarını varsayılan olarak `DateTime` (UTC) döndürür — DTO/record'da `DateTimeOffset` kullanılırsa Dapper record constructor'ını eşleştiremez ve `InvalidOperationException: A parameterless default constructor or one matching signature ... is required` hatasıyla materialize işlemi başarısız olur (Inbound modülünde `GoodsReceiptDto.CreatedAtUtc`/`ApprovedAtUtc` ile karşılaşıldı). Kural: Dapper ile doldurulan tüm DTO'larda tarih alanları `DateTime`/`DateTime?` olacak (değer zaten UTC).

### EF Core Kuralları

- **Tüm entity Id'leri client-side üretilir** (`Guid.CreateVersion7()`, bkz. `BaseEntity`), veritabanı tarafından değil. Bu nedenle her entity konfigürasyonunda **`builder.Property(x => x.Id).ValueGeneratedNever();` zorunludur**. Aksi halde EF Core, zaten set edilmiş (default olmayan) bir Guid değeriyle collection-fixup üzerinden (örn. `aggregate.Children.Add(yeni)`) eklenen yeni bir child entity'yi "Modified" sanıp UPDATE atmaya çalışır, bu da 0 satır etkilenen `DbUpdateConcurrencyException` ile sonuçlanır (Identity modülünde `RefreshToken` eklerken tam olarak bu hatayla karşılaşıldı). Her yeni entity konfigürasyonunda bu satırı eklemeyi unutma.
- **Naming convention**: `EFCore.NamingConventions` paketi + `options.UseSnakeCaseNamingConvention()` ile tüm tablo/kolon adları otomatik snake_case (Postgres konvansiyonu). Manuel `HasColumnName` gerekmez.
- **Optimistic Concurrency**: Aynı satırda **eşzamanlı yarış durumu olabilecek** her yerde optimistic concurrency token kullanılacak — özellikle stok miktarını değiştiren tüm akışlarda (Inbound onayı, Outbound onayı, Transfer gönder/teslim al, StockCount düzeltme onayı → hepsi `Inventory` modülündeki `StockItem.Quantity`'yi günceller). Postgres'in `xmin` sistem kolonu concurrency token olarak kullanılacak — entity konfigürasyonunda `builder.Property<uint>("xmin").IsRowVersion();` (Npgsql EF Core sağlayıcısında ayrı bir `.UseXminAsConcurrencyToken()` extension'ı **yok**, bu shadow-property kalıbı doğru API'dir — Inventory modülü implementasyonunda derleme hatasıyla doğrulandı). Infrastructure katmanındaki repository, `DbUpdateConcurrencyException`'ı yakalayıp EF Core'a bağımlı olmayan `WMS.SharedKernel.ConcurrencyConflictException`'a çevirir; Application katmanındaki handler bunu yakalayıp `Error.Conflict(...)` içeren bir `Result` döner — asla kullanıcıya çıplak 500 olarak yansımaz. Bu kalıp Faz 4'te (Inventory modülü, `StockItem`) uygulandı; salt referans/seed veri (Role gibi) veya tek kullanıcı tarafından değiştirilen kayıtlarda gerekmez.

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
    app/                # Router, providers (QueryClient, ThemeProvider), layout
    features/
      auth/
      products/
      warehouses/
      inbound/
      outbound/
      transfers/
      stock-count/
      reports/
      # her feature: api/ (axios çağrıları + query hooks), components/, types.ts, store.ts (varsa)
    components/ui/      # shadcn bileşenleri
    lib/                 # axios instance, query-client, utils
    hooks/
    types/
```

Naming: API hook'ları `use{Verb}{Resource}` (örn. `useCreateProduct`, `useProducts`), Zustand store'ları `use{Feature}Store`.

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
