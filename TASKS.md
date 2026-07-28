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
- [ ] `StockTransfer`, `StockTransferLine` entity'leri + migration
- [ ] Oluşturma + gönderme (`StockTransferShippedDomainEvent`) + teslim alma (`StockTransferReceivedDomainEvent`) command'ları

## Faz 8 — StockCount Modülü (Sayım + Düzeltme)
- [ ] `StockCount`, `StockCountLine`, `StockCountAdjustment` entity'leri + migration
- [ ] Sayım oturumu açma/kapama, satır girişi, fark hesaplama
- [ ] Düzeltme onay akışı → `StockCountAdjustmentApprovedDomainEvent` → Inventory güncelleme

## Faz 9 — Raporlama
- [ ] Depo bazlı güncel stok raporu (Dapper)
- [ ] Stok hareket geçmişi (ledger) raporu (Dapper)
- [ ] Sayım fark raporu (Dapper)

## Faz 10 — Frontend İskelet
- [ ] Vite + React + TS scaffold
- [ ] shadcn/ui init + Tailwind + dark mode toggle
- [ ] Axios instance (interceptor: JWT header, 401 refresh/redirect)
- [ ] TanStack Query provider
- [ ] Zustand store iskeleti
- [ ] Router + korumalı route yapısı

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
