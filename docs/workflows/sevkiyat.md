# Sevkiyat / Mal Çıkışı (Outbound / GoodsIssue)

## Özet

Depodan mal çıkışı (müşteriye sevkiyat, üretime sarf vb.). Mal Kabul'ün ayna görüntüsü: bir taslak oluşturulur (hangi depodan, hangi üründen kaç adet çıkacağı + serbest metin bir **hedef/açıklama** alanı), bir sorumlu/müdür rolü onaylayınca depodaki stok azalır. Tek fark ve tek ek karmaşıklık: mal kabulün aksine burada **stok gerçekten yeterli mi** sorusu var — depoda olmayan bir ürün sevk edilemez.

## Durum Makinesi

```
Draft (Taslak) --[Approve]--> Approved (Onaylandı)
```

Mal Kabul ile birebir aynı iki durumlu model (bkz. [mal-kabul.md](mal-kabul.md)). Fark, onaydan **önce değil oluşturma anında** yapılan stok kontrolündedir — aşağıya bakınız.

## Roller

| Eylem | Yetkili roller |
|---|---|
| Oluşturma (taslak + satırlar) | **Tüm roller** |
| Onaylama | Admin, DepoMüdürü, DepoSorumlusu (`DepoPersoneli` hariç) |

Mal Kabul ile birebir aynı rol modeli.

## Backend API

Route base: `api/goods-issues` (`backend/src/WMS.Api/Controllers/GoodsIssuesController.cs`)

| Method | Path | Body | Not |
|---|---|---|---|
| `GET` | `/api/goods-issues` | — | Query: `warehouseId?`, `status?` |
| `GET` | `/api/goods-issues/{id}` | — | Tek kayıt, satırlarıyla birlikte |
| `POST` | `/api/goods-issues` | `{warehouseId, destination, lines:[{productId,quantity}]}` | `destination` serbest metin (örn. müşteri adı) |
| `POST` | `/api/goods-issues/{id}/approve` | — (boş) | |

`GoodsIssueDto`, `GoodsReceiptDto`'ya ek olarak `destination: string` alanı içerir.

### Hata kodları

| Durum | `title` | HTTP |
|---|---|---|
| Zaten onaylanmış kaydı tekrar onaylama | `GoodsIssue.NotDraft` | 409 |
| **Oluşturma anında** bir satırın deposunda yeterli stok yok | `GoodsIssue.InsufficientStock` | 409 |
| Var olmayan id | `GoodsIssue.NotFound` | 404 |
| Geçersiz `warehouseId` | `Warehouse.NotFound` | 404 |
| Geçersiz `productId` | `Product.NotFound` | 404 |
| Boş satır / miktar ≤ 0 / boş `destination` (FluentValidation) | `Validation.Failed` | 400 |

### ⚠️ Bilinen kısıt: satır bazlı, toplanmamış stok kontrolü

Bu, projenin en önemli gotcha'larından biri, mutlaka anlaşılmalı: `CreateGoodsIssueCommandHandler` yeterlilik kontrolünü **her satır için bağımsız olarak** yapıyor — aynı depodaki aynı ürünü içeren iki satır varsa, backend bunların miktarlarını **toplamıyor**, her satırı ayrı ayrı mevcut stoğa karşı kontrol ediyor.

Somut örnek: Depoda 10 adet SKU-100 varken, bir sevkiyat talebine "SKU-100 × 6" diye iki ayrı satır eklenirse (toplam 12, depoda 10 var), backend'in mevcut mantığı **her satırı ayrı ayrı** 6 ≤ 10 diye kontrol eder ve **hatalı şekilde kabul eder** — oysa gerçekte depoda 12 adet yok. Bu, backend'in düzeltilmesi gereken bir sınırlamasıdır (kapsam dışı bırakıldı), ama frontend kullanıcıyı bu hataya hiç düşürmeyecek şekilde tasarlandı:

- `NewGoodsIssuePage.tsx`, `ProductLookupDialog`'a her satır için **o ana kadar formda seçilmiş diğer ürünlerin id'lerini** (`excludeProductIds`) geçirir — yani bir ürün bir satırda seçildiyse, diğer satır için açılan seçim penceresinde artık **listelenmez**. Kullanıcı aynı ürünü fiziksel olarak iki satıra koyamaz.
- Ayrıca bir `hasDuplicateProduct` client-side kontrolü, formu submit etmeden önce son bir güvenlik ağı olarak aynı ürünün iki satırda olup olmadığını kontrol eder (örn. tarayıcı geçmişinden geri gelme gibi uç durumlar için).

Yani: **backend'in kendisi bu hatayı önlemez, frontend önler.** Backend'e güvenerek bu dosyanın API tablosunu okuyan biri, bu satırı okumadan "iki satırda aynı ürün olursa ne olur" sorusuna yanlış cevap verebilir — bu yüzden burada açıkça yazılıyor.

### Stok güncellemesi

`GoodsIssueApprovedDomainEventHandler` (Outbound modülünde yaşar) → Inventory'nin `DecreaseStockCommand`'ını çağırır. Mekanizma mal kabulle birebir aynı (bkz. [mal-kabul.md](mal-kabul.md#stok-güncellemesi-nasıl-oluyor) — ayrı transaction, hata durumunda sadece log).

## Frontend

`frontend/src/features/outbound/` — Mal Kabul ile aynı dosya deseni (`types.ts`, `api/goodsIssues.ts`, `GoodsIssuesPage.tsx`, `NewGoodsIssuePage.tsx`), iki ek fark:

- `NewGoodsIssuePage.tsx`'de bir **Hedef** (`destination`) `Input` alanı var (depo seçiminin hemen altında).
- Satır ekleme mantığı yukarıda açıklanan `excludeProductIds`/`hasDuplicateProduct` korumasını içeriyor — bu deseni ilk kuran dosya burasıdır; Transfer'in `NewStockTransferPage.tsx`'i aynısını kopyalar.

Route'lar: `/goods-issues`, `/goods-issues/new`. Nav ve rol-gated Onayla butonu Mal Kabul ile aynı (`useHasAnyRole([Admin, WarehouseManager, WarehouseSupervisor])`).

`lib/errors.ts`'e eklenen Türkçe mesaj: `GoodsIssue.InsufficientStock` → "Depoda yeterli stok bulunmuyor." (kullanıcı gerçek stok miktarı 10 iken 12 istediğinde göreceği mesaj budur — hangi satırın sorunlu olduğunu backend belirtmiyor, bu yüzden mesaj genel tutuldu).

## Doğrulama

Uçtan uca doğrulanan senaryolar:
1. Yeterli stoklu bir üründen normal bir sevkiyat oluşturma → onaylama → depo stoğunun azaldığının Ürünler ekranından teyidi.
2. Aynı ürünü iki satıra eklemeye çalışma → ikinci satırın seçim penceresinde ürünün listelenmediğinin doğrulanması (frontend engeli).
3. Depodaki mevcuttan fazla miktar girme → `409 GoodsIssue.InsufficientStock` → Türkçe hata mesajının doğru gösterilmesi.
