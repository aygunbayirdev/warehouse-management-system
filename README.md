# Warehouse Management System (WMS)

Tek şirket, çoklu depo destekli bir Depo Yönetim Sistemi MVP'si. Ürün/kategori/depo tanımlama, mal kabul, sevkiyat, depolar arası transfer, stok sayımı ve raporlama işlevlerini kapsar.

## Özellikler

| İş akışı | Özet |
|---|---|
| [Mal Kabul](docs/workflows/mal-kabul.md) | Depoya giren malın sisteme işlenmesi (Taslak → Onaylandı) |
| [Sevkiyat](docs/workflows/sevkiyat.md) | Depodan mal çıkışı, stok yeterlilik kontrolü ile |
| [Transfer](docs/workflows/transfer.md) | Depolar arası mal taşıma (Taslak → Gönderildi → Teslim Alındı) |
| [Sayım](docs/workflows/sayim.md) | Fiziksel stok sayımı ve düzeltme onay akışı |

Bunlara ek olarak: Ürün/Kategori/Birim/Depo tanımlama (basit CRUD) ve stok durumu/hareket geçmişi/sayım farkı raporları.

Roller: **Admin**, **DepoMüdürü**, **DepoSorumlusu**, **DepoPersoneli** — her birinin yetkileri [CLAUDE.md §4](CLAUDE.md#4-roller-ve-yetkiler-mvp)'te tanımlı.

## Teknoloji

**Backend**: .NET 10, Clean Architecture + modüler monolith (modül başına PostgreSQL şeması), CQRS (yazma: EF Core, okuma: Dapper), MediatR (in-process domain event'ler), JWT Bearer kimlik doğrulama.

**Frontend**: React + Vite + TypeScript, TanStack Query (sunucu state'i), Zustand (istemci state'i), shadcn/ui + Tailwind CSS (dark/light tema).

Mimari kararların ve konvansiyonların tam listesi için bkz. [CLAUDE.md](CLAUDE.md).

## Başlarken

### Yol 1 — Docker Compose (en hızlısı, tam stack)

```bash
cp .env.example .env
docker compose up -d
```

Frontend `http://localhost:3000`, backend `http://localhost:5000/api` adresinde açılır (portlar `.env`'de değiştirilebilir). İlk açılışta (veritabanı tamamen boşsa):
- Veritabanı şemaları ve varsayılan Admin kullanıcısı (`admin@wms.local` / `ChangeMe123!`) otomatik oluşturulur.
- Tutarlı bir **demo veri seti** otomatik yüklenir (birkaç ürün/depo/onaylanmış işlem, kasıtlı olarak bırakılmış bir taslak transfer ve bir onay bekleyen sayım düzeltmesi dahil) — böylece ilk açılışta sistem boş bir kabuk değil, gerçekçi ve tutarlı sayılarla dolu, denenebilir bir uygulama olarak karşılar. Bu veri sadece **veritabanı tamamen boşken** bir kere yüklenir, var olan gerçek veriyi asla ezmez; `appsettings.json` → `Seeding:SeedDemoData` ile kapatılabilir.

### Yol 2 — Lokal geliştirme (hot reload, ayrı ayrı)

Postgres'i ayağa kaldırın (sadece bu servis için Docker Compose kullanılabilir):

```bash
docker compose up -d postgres
```

Backend:

```bash
cd backend
dotnet run --project src/WMS.Api
```

API `http://localhost:5088` adresinde (bkz. `backend/src/WMS.Api/Properties/launchSettings.json`). Detaylar için [backend/README.md](backend/README.md).

Frontend (ayrı bir terminalde):

```bash
cd frontend
npm install
cp .env.example .env.development
npm run dev
```

`http://localhost:5173` adresinde açılır. Detaylar için [frontend/README.md](frontend/README.md).

## Test

```bash
# Backend: unit + entegrasyon (Testcontainers, Docker gerektirir) + functional testler
cd backend && dotnet test WMS.slnx

# Frontend: Vitest + React Testing Library
cd frontend && npm run test
```

## Dokümantasyon Haritası

- **[CLAUDE.md](CLAUDE.md)** — mimari kararlar, naming standardı, teknoloji yığını, roller, geliştirme döngüsü. Yeni kod yazmadan önce okunması gereken proje rehberi.
- **[docs/workflows/](docs/workflows/)** — her iş akışının durum makinesi, backend API sözleşmesi, frontend tasarım kararları ve bilinen kısıtları dahil derinlemesine dokümantasyonu.
- **[TASKS.md](TASKS.md)** — MVP'nin tüm fazlarının kronolojik geliştirme geçmişi (ne, ne zaman, neden yapıldı).
