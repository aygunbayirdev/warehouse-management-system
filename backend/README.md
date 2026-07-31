# WMS Backend

.NET 10 ile yazılmış, Clean Architecture + modüler monolith mimarisinde bir Depo Yönetim Sistemi API'si. Mimari kararlar, CQRS/domain-event kalıpları ve naming standardı için bkz. repo kökündeki [CLAUDE.md](../CLAUDE.md).

## Kurulum

PostgreSQL'e ihtiyaç var (repo kökündeki `docker-compose.yml` ile sadece bu servis ayağa kaldırılabilir):

```bash
cd ..
docker compose up -d postgres
```

## Çalıştırma

```bash
dotnet run --project src/WMS.Api
```

API `http://localhost:5088` adresinde açılır (bkz. `src/WMS.Api/Properties/launchSettings.json`). İlk açılışta:
- Her modülün veritabanı migration'ları otomatik uygulanır (7 şema: `identity`, `catalog`, `inventory`, `inbound`, `outbound`, `transfer`, `stockcount`) — ayrı bir `dotnet ef database update` adımına gerek yok.
- Varsayılan bir Admin kullanıcısı seed edilir: `admin@wms.local` / `ChangeMe123!` (bkz. `appsettings.json` → `Identity:AdminSeed` — üretimde değiştirilmeli).
- Tutarlı bir demo veri seti (`src/WMS.Api/Seeding/DemoDataSeeder.cs`) otomatik yüklenir — birim/kategori/ürün/depo referans verisi + onaylanmış mal kabul/sevkiyat/transfer + bir taslak transfer + bir onay bekleyen sayım düzeltmesi. Ham SQL değil, gerçek `ISender`/MediatR command'ları üzerinden oluşturulduğu için stok miktarları/ledger/sayım farkları arasında tutarsızlık riski yoktur. Sadece veritabanı tamamen boşken çalışır (mevcut veriyi asla ezmez); `appsettings.json` → `Seeding:SeedDemoData` (varsayılan `true`) ile kapatılabilir.

Bağlantı dizesi ve JWT secret'ı `appsettings.json`'da tanımlıdır; `ConnectionStrings__Default`/`Jwt__Secret` ortam değişkenleriyle override edilebilir (Docker Compose'un yaptığı budur, bkz. repo kökündeki `docker-compose.yml`).

## Test

```bash
dotnet test WMS.slnx
```

Üç katman:
- **Unit** (`tests/WMS.Modules.*.UnitTests`) — handler'lar, mock repository/`ISender` ile (xUnit + FluentAssertions + NSubstitute), Docker gerektirmez.
- **Entegrasyon** (`tests/WMS.Modules.*.IntegrationTests`) — gerçek Postgres'e karşı EF write + Dapper read repository'leri (Testcontainers.PostgreSql) — **Docker çalışıyor olmalı**.
- **Functional** (`tests/WMS.Api.FunctionalTests`) — gerçek HTTP istekleriyle tüm modüller (WebApplicationFactory + Testcontainers) — **Docker çalışıyor olmalı**.

Sadece unit testleri çalıştırmak için (Docker gerekmez):

```bash
dotnet test WMS.slnx --filter "FullyQualifiedName!~IntegrationTests&FullyQualifiedName!~FunctionalTests"
```

## Migration Ekleme

Her modülün kendi `DbContext`'i ve migration klasörü var. Yeni bir migration eklemek için (örnek: Catalog):

```bash
dotnet tool install --global dotnet-ef   # bir kere
dotnet ef migrations add <MigrationAdi> \
  --project src/Modules/Catalog/WMS.Modules.Catalog.Infrastructure \
  --startup-project src/WMS.Api \
  --context CatalogDbContext
```

Migration'lar uygulama başlangıcında otomatik uygulanır (bkz. `src/WMS.Api/Program.cs`), manuel `dotnet ef database update` gerekmez.

## Proje Yapısı

```
backend/
  src/
    WMS.Api/                # Composition root: controller'lar, middleware, DI, appsettings
    WMS.SharedKernel/        # BaseEntity, Result<T>, Error, Guard
    BuildingBlocks/          # MediatR pipeline behavior'ları, domain-event dispatch, Dapper connection factory
    Modules/                 # Identity, Catalog, Inventory, Inbound, Outbound, Transfer, StockCount
                             # her biri: Domain / Application / Infrastructure
  tests/
    WMS.Modules.*.UnitTests
    WMS.Modules.*.IntegrationTests
    WMS.Api.FunctionalTests
```

Detaylı mimari kararlar (modül sınırları, CQRS ayrımı, domain event akışı, naming standardı) için [../CLAUDE.md](../CLAUDE.md).
