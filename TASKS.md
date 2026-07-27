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
- [ ] Solution dosyası ve proje şablonları (WMS.Api, WMS.SharedKernel, WMS.BuildingBlocks.Application, modül proje iskeletleri)
- [ ] SharedKernel: `BaseEntity`, `IDomainEvent`, `Result<T>`, Guard yardımcıları
- [ ] MediatR pipeline behaviors: validation (FluentValidation), logging
- [ ] Domain event dispatch mekanizması (SaveChangesAsync sonrası MediatR publish)
- [ ] Global exception handling middleware
- [ ] Serilog kurulumu
- [ ] appsettings + DI kayıt iskeleti (modül bazlı `AddXxxModule()` extension'ları)

## Faz 2 — Identity & Auth Modülü
- [ ] `User`, `Role`, `Permission`, `UserRole`, `UserWarehouse` entity'leri
- [ ] EF Core migration (identity schema)
- [ ] JWT issuing (access + refresh token)
- [ ] Login / refresh endpoint
- [ ] Role-based authorization policy'leri
- [ ] Seed: Admin kullanıcı + 4 rol (Admin, DepoMüdürü, DepoSorumlusu, DepoPersoneli)

## Faz 3 — Catalog Modülü
- [ ] `Product`, `Category`, `UnitOfMeasure` entity'leri + migration
- [ ] Command/Query/DTO/Validator/Repository (CRUD)
- [ ] Controller endpoint'leri

## Faz 4 — Inventory Modülü
- [ ] `Warehouse`, `StockItem`, `StockMovement` entity'leri + migration
- [ ] Warehouse CRUD
- [ ] Dapper ile stok seviyesi read-repository / query'leri

## Faz 5 — Inbound Modülü (Mal Kabul)
- [ ] `GoodsReceipt`, `GoodsReceiptLine` entity'leri + migration
- [ ] Oluşturma + onaylama command'ları
- [ ] `GoodsReceiptApprovedDomainEvent` → Inventory güncelleme handler'ı

## Faz 6 — Outbound Modülü (Sevkiyat / Mal Çıkışı)
- [ ] `GoodsIssue`, `GoodsIssueLine` entity'leri + migration
- [ ] Oluşturma (stok yeterlilik kontrolü) + onaylama command'ları
- [ ] `GoodsIssueApprovedDomainEvent` → Inventory güncelleme handler'ı

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
