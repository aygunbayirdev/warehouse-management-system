# WMS Frontend

React + Vite + TypeScript ile yazılmış Depo Yönetim Sistemi arayüzü. Mimari kararlar ve klasör yapısı için bkz. repo kökündeki [CLAUDE.md](../CLAUDE.md); her iş akışının ekran/tasarım kararları için [docs/workflows/](../docs/workflows/).

## Kurulum

```bash
npm install
cp .env.example .env.development
```

`.env.development` dosyasında API adresi tanımlıdır (`VITE_API_URL`, gitignore'da — bkz. `.env.example`). Varsayılan olarak backend'in `http` profiliyle (`http://localhost:5088/api`) eşleşir; farklı bir port/host kullanıyorsanız güncelleyin.

## Çalıştırma

```bash
npm run dev
```

Vite dev server `http://localhost:5173` adresinde açılır. Backend'in bu origin'e CORS izni vermesi gerekir (bkz. `backend/src/WMS.Api/appsettings.json` → `Cors:AllowedOrigins`).

Giriş için varsayılan Admin hesabı: `admin@wms.local` / `ChangeMe123!` (backend'in seed ettiği, bkz. `backend/README.md`).

## Test

```bash
npm run test
```

Vitest + React Testing Library ile çalışır. Her feature alanı için temsili smoke test'ler var (auth, ürün/kategori/birim CRUD, dört iş akışının tamamı, raporlar, paylaşılan `lib/` yardımcıları) — kapsamlı/eksiksiz test değil, CLAUDE.md'nin MVP test felsefesiyle tutarlı.

## Build

```bash
npm run build
```

`tsc -b` ile tip kontrolü yapıp ardından Vite prod build üretir (`dist/`).

## Proje Yapısı

```
src/
  app/            # App.tsx, router.tsx, AppLayout, ThemeProvider, ProtectedRoute/RoleGuard
  features/       # auth, products, warehouses, inbound, outbound, transfer, stock-count, reports
                  #   her biri: api/ (TanStack Query hook'ları), types.ts, {Feature}Page.tsx
  components/ui/  # shadcn bileşenleri (Base UI tabanlı)
  lib/            # axios, query-client, errors, dates, pagination, utils
```

Naming ve state-yönetimi kuralları (TanStack Query vs Zustand ayrımı, sayfa/hook isimlendirme) için bkz. [../CLAUDE.md](../CLAUDE.md) §2 ve §3.

## Notlar

- Tema (dark/light): `localStorage` + `prefers-color-scheme`, `<html class="dark">` ile Tailwind/shadcn `dark:` varyantı tetiklenir.
- Kimlik doğrulama: backend'den gelen JWT access/refresh token'ları Zustand + `persist` middleware ile `localStorage`'da tutulur (`features/auth/`); giriş yapmış kullanıcının profili ise TanStack Query ile (`useCurrentUser`) — backend'den gelen hiçbir veri Zustand'da tutulmaz, bu kesin bir kural (bkz. CLAUDE.md "Frontend Kuralları").
- 401 alındığında axios interceptor otomatik refresh token akışını dener, başarısız olursa `/login`'e yönlendirir (bkz. `lib/axios.ts`).
