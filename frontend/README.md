# WMS Frontend

React + Vite + TypeScript ile yazılmış Depo Yönetim Sistemi arayüzü. Mimari kararlar ve klasör yapısı için bkz. repo kökündeki `CLAUDE.md`.

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

## Test

```bash
npm run test
```

Vitest + React Testing Library ile çalışır. Bu aşamada (Faz 10 — İskelet) sadece axios interceptor ve `ProtectedRoute` için smoke testler var; kapsamlı özellik testleri Faz 14'te eklenecek.

## Build

```bash
npm run build
```

`tsc -b` ile tip kontrolü yapıp ardından Vite prod build üretir (`dist/`).

## Notlar

- Tema (dark/light): `localStorage` + `prefers-color-scheme`, `<html class="dark">` ile Tailwind/shadcn `dark:` varyantı tetiklenir.
- Kimlik doğrulama state'i (`accessToken`/`refreshToken`/`user`) Zustand + `persist` middleware ile `localStorage`'da tutulur (`features/auth/store.ts`).
- Gerçek login ekranı ve akışı Faz 11'de eklenecek; şu an `/login` yalnızca bir placeholder.
