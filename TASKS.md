# WMS — MVP Görev Listesi

Geliştirme döngüsü: Planla → Implement et → Testleri güncelle/yaz → Dökümanları güncelle/yaz → Commit mesajı yaz ve review için sun → Onay al → Commit et → aşağıda `[x]` olarak işaretle.

Mimari/teknoloji/naming detayları için bkz. `CLAUDE.md`.

## Faz 0 — Repo İskeleti
- [x] `frontend/` ve `backend/` klasörleri
- [x] `.gitignore`, `.editorconfig`
- [x] `docker-compose.yml` iskeleti (postgres servisi)
- [x] `CLAUDE.md`
- [x] `TASKS.md`
- [x] Git repository başlatma ve ilk commit

## Faz 1 — Backend Çekirdek
- [x] Solution dosyası ve proje şablonları (WMS.Api, WMS.SharedKernel, WMS.BuildingBlocks.Application, WMS.BuildingBlocks.Infrastructure, modül proje iskeletleri — 7 modül × Domain/Application/Infrastructure)
- [x] SharedKernel: `BaseEntity`, `IDomainEvent`, `Result`/`Result<T>`, `Error`, Guard yardımcıları
- [x] MediatR pipeline behaviors: validation (FluentValidation), logging
- [x] Domain event dispatch mekanizması (SaveChangesAsync sonrası MediatR publish — `DomainEventDispatchInterceptor`)
- [x] Global exception handling middleware (`IExceptionHandler` + ProblemDetails)
- [x] Serilog kurulumu
- [x] appsettings + DI kayıt iskeleti (modül bazlı `AddXxxModule()` extension'ları, her modül kendi Application assembly'sinden MediatR/FluentValidation kaydı yapıyor)

## Faz 2 — Identity & Auth Modülü
- [x] `User`, `Role`, `UserRole`, `RefreshToken` entity'leri (Permission entity kapsam dışı bırakıldı — rol bazlı yetkilendirme yeterli; `UserWarehouse` Faz 4'e ertelendi, bkz. CLAUDE.md §5)
- [x] EF Core migration (identity schema, snake_case naming convention)
- [x] JWT issuing (access + refresh token, refresh token rotation)
- [x] Login / refresh endpoint (`POST /api/auth/login`, `POST /api/auth/refresh`, `GET /api/auth/me`)
- [x] Role-based authorization policy'leri (JWT bearer + `[Authorize(Roles=...)]`, doğrulandı)
- [x] Seed: Admin kullanıcı + 4 rol (Admin, DepoMüdürü, DepoSorumlusu, DepoPersoneli) — uygulama başlangıcında idempotent seed

## Faz 3 — Catalog Modülü
- [x] `Product`, `Category`, `UnitOfMeasure` entity'leri + migration (catalog schema, snake_case)
- [x] Command/Query/DTO/Validator/Repository (CRUD) — EF Core write + Dapper read (denormalized join for `ProductDto`)
- [x] Controller endpoint'leri (`ProductsController`, `CategoriesController`, `UnitsOfMeasureController`); okuma tüm kullanıcılara, yazma Admin/DepoMüdürü'ne açık
- [x] Referans bütünlüğü: Product'ta kullanılan UnitOfMeasure/Category silinemez (`Error.Conflict`, DB'de `DeleteBehavior.Restrict` ile ikinci savunma katmanı) — uçtan uca doğrulandı

## Faz 4 — Inventory Modülü
- [x] `Warehouse`, `StockItem`, `StockMovement` entity'leri + migration (inventory schema, snake_case; `StockItem` Postgres `xmin` ile optimistic concurrency — bkz. CLAUDE.md "EF Core Kuralları")
- [x] Warehouse CRUD (`WarehousesController`; okuma tüm kullanıcılara, yazma Admin/DepoMüdürü'ne açık; silme StockItem varlığıyla korunuyor)
- [x] Dapper ile stok seviyesi read-repository / query'leri (`GetStockItemsQuery`, catalog şeması ile cross-schema join — CLAUDE.md'de tanımlı pragmatik istisna)
- [x] `IncreaseStockCommand`/`DecreaseStockCommand`: modüller arası genel stok değiştirme API'si (Faz 5+'ta Inbound/Outbound/Transfer/StockCount tarafından MediatR ile çağrılacak), `StockMovement` audit ledger'ı otomatik yazıyor, concurrency çakışması `Error.Conflict`'e çevriliyor — `StockController`'da Admin-only manuel düzeltme endpoint'i (`/api/stock/increase`, `/api/stock/decrease`) olarak da erişilebilir; uçtan uca doğrulandı (yetersiz stok, concurrency, duplicate code, validation, 401 dahil)

## Faz 5 — Inbound Modülü (Mal Kabul)
- [x] `GoodsReceipt`, `GoodsReceiptLine` entity'leri + migration (inbound schema, snake_case; Status: Draft/Approved)
- [x] Oluşturma (WarehouseId/ProductId varlığı Inventory/Catalog'un query'leri ile doğrulanıyor) + onaylama command'ları (`CreateGoodsReceiptCommand`, `ApproveGoodsReceiptCommand`; onay DepoSorumlusu/DepoMüdürü/Admin'e açık)
- [x] `GoodsReceiptApprovedDomainEvent` → Inventory güncelleme handler'ı (`GoodsReceiptApprovedDomainEventHandler`, Inbound modülünde yaşıyor, Inventory'nin `IncreaseStockCommand`'ını `ISender` ile çağırıyor — modüller arası çağrı kalıbı ilk kez burada kuruldu, bkz. CLAUDE.md §1 "Modüller arası çağrı kalıbı")
- [x] Uçtan uca doğrulandı: taslak oluşturma → onay → Inventory'de stok artışı ve `StockMovement` kaydı, çift onay/geçersiz ürün/depo/rol koruması dahil

## Faz 6 — Outbound Modülü (Sevkiyat / Mal Çıkışı)
- [x] `GoodsIssue`, `GoodsIssueLine` entity'leri + migration (outbound schema, snake_case; Status: Draft/Approved)
- [x] Oluşturma (WarehouseId/ProductId varlığı Inventory/Catalog'un query'leri ile doğrulanıyor; stok yeterlilik kontrolü `GetStockItemsQuery` ile satır bazında oluşturma anında yapılıyor — onay sonrası domain event handler'ın sessizce loglayıp geçmesi riskine karşı erken geri bildirim) + onaylama command'ları (`CreateGoodsIssueCommand`, `ApproveGoodsIssueCommand`; onay DepoSorumlusu/DepoMüdürü/Admin'e açık)
- [x] `GoodsIssueApprovedDomainEvent` → Inventory güncelleme handler'ı (`GoodsIssueApprovedDomainEventHandler`, Outbound modülünde yaşıyor, Inventory'nin `DecreaseStockCommand`'ını `ISender` ile çağırıyor — Faz 5'te kurulan modüller arası çağrı kalıbının ikinci uygulaması)
- [x] Uçtan uca doğrulandı: yetersiz stokta 409, taslak oluşturma → onay → Inventory'de stok azalışı, çift onay/geçersiz ürün/depo/eksik satır/401 koruması dahil

## Faz 7 — Transfer Modülü (Depolar Arası)
- [x] `StockTransfer`, `StockTransferLine` entity'leri + migration (transfer schema, snake_case; Status: Draft/Shipped/Received/Cancelled — Cancelled MVP'de bir command ile tetiklenmiyor, ileride kullanılmak üzere şema seviyesinde ayrılmış durumda)
- [x] Oluşturma (Kaynak/Hedef depo aynı olamaz, WarehouseId/ProductId varlığı Inventory/Catalog'un query'leri ile doğrulanıyor) + gönderme (`ShipStockTransferCommand`; stok yeterlilik kontrolü `GetStockItemsQuery` ile gönderme anında yapılıyor — Outbound'daki yaklaşımla aynı gerekçe, `StockTransferShippedDomainEvent`) + teslim alma (`ReceiveStockTransferCommand`, `StockTransferReceivedDomainEvent`) command'ları; gönderme/teslim alma DepoSorumlusu/DepoMüdürü/Admin'e açık
- [x] `StockTransferShippedDomainEventHandler` (Inventory'nin `DecreaseStockCommand`'ını kaynak depo için çağırıyor) ve `StockTransferReceivedDomainEventHandler` (Inventory'nin `IncreaseStockCommand`'ını hedef depo için çağırıyor) — ikisi de Transfer modülünde yaşıyor, Faz 5/6'da kurulan modüller arası çağrı kalıbının üçüncü ve dördüncü uygulaması
- [x] Uçtan uca doğrulandı: aynı depo validasyonu, gönderim anında yetersiz stokta 409, taslak → gönder (kaynak stok azalır) → teslim al (hedef stok artar), teslim almadan önce gönderilmemiş transferin reddedilmesi, çift gönderme/çift teslim alma/geçersiz depo/boş satır/401 koruması dahil

## Faz 8 — StockCount Modülü (Sayım + Düzeltme)
- [x] `StockCount` (Draft/InProgress/Completed), `StockCountLine` (SistemMiktarı satır girişinde Inventory'den okunup dondurulur, Fark = SayılanMiktar - SistemMiktarı), `StockCountAdjustment` (ayrı aggregate root, Pending/Approved/Rejected) entity'leri + migration (stockcount schema, snake_case)
- [x] Sayım oturumu açma (`CreateStockCountCommand`, Draft) / başlatma (`StartStockCountCommand`, InProgress — DepoSorumlusu/DepoMüdürü/Admin'e açık) / kapama (`CompleteStockCountCommand`, Completed); satır girişi (`SubmitStockCountLineCommand`, tüm rollere açık, aynı üründen ikinci satır engelleniyor); fark hesaplama satır girişi anında Inventory'nin `GetStockItemsQuery`'si ile yapılıyor
- [x] Sayım kapatıldığında farkı sıfır olmayan her satır için otomatik olarak Pending bir `StockCountAdjustment` oluşuyor (aynı transaction, aynı DbContext — modüller arası olmadığı için domain event'e gerek yok); düzeltme onay/red akışı (`ApproveStockCountAdjustmentCommand`/`RejectStockCountAdjustmentCommand`, DepoMüdürü/Admin'e açık) → onayda `StockCountAdjustmentApprovedDomainEvent` → Inventory'nin `IncreaseStockCommand`/`DecreaseStockCommand`'ından farkın işaretine göre doğru olanı çağıran handler (Faz 5/6/7'de kurulan modüller arası çağrı kalıbının beşinci uygulaması)
- [x] Uçtan uca doğrulandı: InProgress olmadan satır girişi reddi, duplicate ürün satırı reddi, sıfır fark → adjustment oluşturulmaması, pozitif ve negatif farkların doğru şekilde stok artış/azalışına yansıması, red edilen düzeltmenin stoku değiştirmemesi, satırsız sayımın kapatılamaması, geçersiz depo/401 koruması dahil
- [x] `WMS.Modules.StockCount` modül namespace'i ile aggregate root sınıfı `StockCount` aynı ada sahip olduğu için (`WMS.Modules.StockCount.Domain.StockCount`), `using WMS.Modules.StockCount.Domain;` + çıplak `StockCount` kullanımı C# derleyicisinde CS0118 ("ad alanı öğesi tür olarak kullanılıyor") hatası veriyor — çünkü isim çözümleme, `WMS.Modules` seviyesinde `StockCount` adlı bir alt ad alanını (modülün kendisini) tür aramasından önce buluyor. Çözüm: bu sınıfa referans veren her dosyada `using StockCountAggregate = WMS.Modules.StockCount.Domain.StockCount;` alias'ı kullanılıyor (bkz. `IStockCountWriteRepository`, `StockCountWriteRepository`, `StockCountDbContext`, `StockCountConfiguration`, `CreateStockCountCommandHandler`). Modül adı ile o modüldeki bir aggregate root'un adı birebir aynı olursa bu kalıp tekrar gerekir.

## Faz 9 — Raporlama
- [x] Depo bazlı güncel stok raporu (Dapper) — Faz 4'te `GetStockItemsQuery` / `GET /api/stock` ile zaten mevcuttu, ayrı bir Reports modülü yok (mimaride 7 modül dışında raporlama modülü tanımlanmadı) — her rapor ilgili modülün Dapper read-repository'sine ekleniyor
- [x] Stok hareket geçmişi (ledger) raporu (Dapper) — Inventory modülünde yeni `GetStockMovementsQuery`/`IStockMovementReadRepository`/`StockMovementReadRepository`, `inventory.stock_movements` tablosunu `catalog.products`/`inventory.warehouses` ile join'liyor, depo/ürün/tarih aralığı filtreli, `GET /api/stock/movements`; Inbound/Outbound/Transfer/StockCount'un ürettiği tüm `StockMovement` kayıtları tek ledger'da görünüyor — uçtan uca doğrulandı
- [x] Sayım fark raporu (Dapper) — StockCount modülünde yeni `GetStockCountVarianceReportQuery`, `IStockCountReadRepository.GetVarianceReportAsync`, sadece Completed sayımların farkı sıfır olmayan satırlarını depo/tarih aralığı filtreli döndürüyor, `GET /api/stock-counts/variance-report`; rapor, düzeltmenin onay/red durumundan bağımsız olarak sayımda tespit edilen farkı gösteriyor — uçtan uca doğrulandı

## Faz 10 — Frontend İskelet
- [x] Vite + React + TS scaffold (`frontend/`, Vite 8 + React 19 + TS 6, `oxlint`)
- [x] shadcn/ui init (Base UI + Nova preset) + Tailwind CSS v4 (`@tailwindcss/vite`) + dark mode toggle (`ThemeProvider` + `ThemeToggle`, `localStorage` + `prefers-color-scheme`, `<html class="dark">` üzerinden Tailwind `dark:` varyantı)
- [x] Axios instance (`lib/axios.ts`): request interceptor JWT header ekliyor (`attachAuthHeader`), response interceptor 401'de `/auth/refresh` deniyor, başarısız olursa store'u temizleyip `/login`'e yönlendiriyor
- [x] TanStack Query provider (`lib/query-client.ts`, `app/App.tsx`)
- [x] Zustand store iskeleti (`features/auth/store.ts`, `persist` middleware ile `localStorage`)
- [x] Router + korumalı route yapısı (`app/router.tsx`, `app/routes/ProtectedRoute.tsx`, `app/routes/RoleGuard.tsx`) — gerçek login ekranı ve özellik sayfaları henüz yok (Faz 11/12), `/login` ve `/` şimdilik placeholder
- [x] Backend ön koşulu: CORS policy eklendi (`Program.cs`/`appsettings.json` → `Cors:AllowedOrigins`, dev origin `http://localhost:5173`) — backend'de daha önce hiç CORS yapılandırması yoktu
- [x] Uçtan uca doğrulandı: `dotnet build`, `npm run build`, `npm run test` (4/4 yeşil), dev server'da `/`→`/login` redirect, tema toggle `<html class="dark">` değişimi, gerçek backend'e karşı CORS hatasız cross-origin istek ve `admin@wms.local` ile login'in geçerli JWT (`role` claim'i dahil) döndürmesi

## Faz 11 — Frontend Auth
- [ ] Login sayfası
- [ ] Token saklama + refresh akışı
- [ ] Route guard (rol bazlı)
- [ ] Rol bazlı UI gizleme/gösterme

## Faz 12 — Frontend Features
- [ ] Ürünler (Catalog) ekranları
- [ ] Depolar (Warehouse) ekranları
- [ ] Mal Kabul (Inbound) ekranları
- [ ] Sevkiyat (Outbound) ekranları
- [ ] Transfer ekranları
- [ ] Sayım (Stock Count) ekranları
- [ ] Raporlar ekranları

## Faz 13 — Docker Compose Entegrasyonu
- [ ] `backend/Dockerfile` (multi-stage .NET build)
- [ ] `frontend/Dockerfile` (multi-stage Vite build → nginx)
- [ ] `docker-compose.yml` tam stack (postgres + backend + frontend)
- [ ] `docker compose up` ile uçtan uca doğrulama

## Faz 14 — Test & Doküman Tamamlama
- [ ] Backend unit testleri (handler bazlı, tüm modüller)
- [ ] Backend entegrasyon testleri (Testcontainers, EF + Dapper repository'leri)
- [ ] Backend API functional testleri (WebApplicationFactory)
- [ ] Frontend testleri (Vitest + RTL, feature bazlı)
- [ ] `CLAUDE.md` / `TASKS.md` güncel tutulması
