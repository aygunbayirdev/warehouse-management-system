# Mal Kabul (Inbound / GoodsReceipt)

## Özet

Depoya giren malın sisteme işlenmesi. Herhangi bir rol bir mal kabul **taslağı** oluşturabilir (hangi depoya, hangi üründen kaç adet geldiğini satır satır girer); bir sorumlu/müdür rolü bunu **onaylayınca** depodaki stok miktarı otomatik artar. Bu, projedeki en basit iki-durumlu workflow'dur — Sevkiyat ve Transfer'in de temel aldığı desenin ilk uygulaması.

En basit sebep-sonuç: Taslak oluşturma bir "niyet beyanı"dır, henüz stoğa dokunmaz. Onay, geri dönüşü olmayan gerçek stok hareketini tetikleyen andır. Bu ayrım MVP'nin her stok hareketi modülünde (Mal Kabul/Sevkiyat/Transfer/Sayım) tekrar eder: veri girişi ile onay/gönderim/teslim farklı kişiler/roller tarafından yapılabilsin diye ayrıştırılmıştır (CLAUDE.md §4'teki rol tablosuna bakınız — DepoPersoneli veri girer, DepoSorumlusu/DepoMüdürü onaylar).

## Durum Makinesi

```
Draft (Taslak) --[Approve]--> Approved (Onaylandı)
```

Sadece iki durum vardır. Düzenleme veya silme yoktur — bir taslakta hata varsa yeniden oluşturmak gerekir (MVP kapsamında bu kabul edilmiş bir sınırlamadır). `Cancelled` gibi üçüncü bir durum da yoktur (Transfer'de olduğu gibi şema seviyesinde bile ayrılmamış).

## Roller

| Eylem | Yetkili roller |
|---|---|
| Oluşturma (taslak + satırlar) | **Tüm roller** (Admin, DepoMüdürü, DepoSorumlusu, DepoPersoneli) |
| Onaylama | Admin, DepoMüdürü, DepoSorumlusu (`DepoPersoneli` **hariç**) |

Backend'de bu, `GoodsReceiptsController`'da şöyle görünür:

```csharp
private const string ApproveRoles = $"{RoleNames.Admin},{RoleNames.WarehouseManager},{RoleNames.WarehouseSupervisor}";
```

Oluşturma action'ında (`[HttpPost]`) hiçbir `[Authorize(Roles=...)]` yok — controller'ın sınıf seviyesindeki çıplak `[Authorize]` yeterli, yani herhangi bir authenticated kullanıcı satır girebilir. Bu bilinçli bir tasarım: depo sahasındaki personelin (DepoPersoneli) veri girmesi, ofisteki bir sorumlunun onaylaması senaryosunu yansıtıyor.

## Backend API

Route base: `api/goods-receipts` (`backend/src/WMS.Api/Controllers/GoodsReceiptsController.cs`)

| Method | Path | Body | Not |
|---|---|---|---|
| `GET` | `/api/goods-receipts` | — | Query: `warehouseId?`, `status?` (`Draft`/`Approved`) |
| `GET` | `/api/goods-receipts/{id}` | — | Tek kayıt, satırlarıyla birlikte |
| `POST` | `/api/goods-receipts` | `{warehouseId, lines:[{productId,quantity}]}` | `createdByUserId` JWT'den alınır, body'de gönderilmez |
| `POST` | `/api/goods-receipts/{id}/approve` | — (boş) | Sadece route'taki `id` yeterli |

Response şekli (`GoodsReceiptDto`):
```json
{
  "id": "guid",
  "warehouseId": "guid",
  "warehouseName": "string",
  "status": "Draft",
  "createdByUserId": "guid",
  "createdAtUtc": "2026-01-01T10:00:00",
  "approvedAtUtc": null,
  "lines": [
    { "productId": "guid", "productSku": "string", "productName": "string", "quantity": 12.5 }
  ]
}
```
`warehouseName`/`productSku`/`productName` **denormalize** edilmiş alanlardır (Dapper okuma tarafı `catalog`/`inventory` şemalarını join'ler) — frontend'in ayrıca ürün/depo adı için ek bir sorgu atmasına gerek yoktur. `createdAtUtc`/`approvedAtUtc` — CLAUDE.md'nin Dapper kuralı gereği `DateTime` (Z'siz UTC).

### Hata kodları

| Durum | `title` | HTTP |
|---|---|---|
| Zaten onaylanmış bir kaydı tekrar onaylama | `GoodsReceipt.NotDraft` | 409 |
| Onaylarken satır sayısı 0 (pratikte imkansız — create zaten boş satırı reddediyor) | `GoodsReceipt.NoLines` | 400 |
| Var olmayan id | `GoodsReceipt.NotFound` | 404 |
| Geçersiz `warehouseId` | `Warehouse.NotFound` | 404 |
| Bir satırdaki geçersiz `productId` | `Product.NotFound` | 404 |
| Boş satır listesi / miktar ≤ 0 / boş `warehouseId` (FluentValidation) | `Validation.Failed` | 400 |

Mal Kabul'de **stok yeterlilik kontrolü yoktur** — malın depoya girdiği bir işlemde "yeterli stok" diye bir kavram olmadığı için bu modülün doğal bir özelliği (Sevkiyat/Transfer'in aksine).

### Stok güncellemesi nasıl oluyor?

Onay anında `GoodsReceiptApprovedDomainEvent` yayınlanır. Bu event'i dinleyen handler **Inbound modülünün kendi içinde yaşar** (`GoodsReceiptApprovedDomainEventHandler`), Inventory modülünün `IncreaseStockCommand`'ını `ISender` ile çağırır. Bu, CLAUDE.md §1'de tanımlanan "modüller arası çağrı kalıbı"nın ilk kurulduğu yerdir — event'i üreten modül (Inbound) tüketiciyi (Inventory) çağırır, tersi değil. Bu komut kendi ayrı transaction'ında çalışır; başarısız olursa (örn. bir concurrency çakışması) sadece loglanır, onay işlemi 500'e dönüşmez — bu MVP'nin bilinçli olarak kabul ettiği bir "eventual consistency" riskidir.

## Frontend

`frontend/src/features/inbound/`:

| Dosya | Görev |
|---|---|
| `types.ts` | `GoodsReceiptDto`, `GoodsReceiptStatus`, `CreateGoodsReceiptPayload` — backend sözleşmesiyle birebir |
| `api/goodsReceipts.ts` | `useGoodsReceipts(filters)`, `useCreateGoodsReceipt()`, `useApproveGoodsReceipt()` |
| `GoodsReceiptsPage.tsx` | Liste (depo/durum filtresi, durum rozeti) + detay `Dialog`'u (satırlar + `Draft` ise Onayla butonu) + onay `AlertDialog`'u |
| `NewGoodsReceiptPage.tsx` | `/goods-receipts/new` — depo seçimi + dinamik satırlar (`ProductLookupDialog` ile ürün seçimi) |

Route'lar: `/goods-receipts` (liste), `/goods-receipts/new` (oluşturma) — ikisi de rol kısıtı olmadan `ProtectedRoute` altında (backend'in "oluşturma tüm rollere açık" kuralıyla tutarlı).

"Onayla" butonu sadece `useHasAnyRole([Admin, WarehouseManager, WarehouseSupervisor])` `true` dönerse görünür — backend'in `ApproveRoles`'uyla birebir aynı liste, frontend'de ayrıca yazılmıştır (backend rol kontrolünü **tekrarlar**, onun yerine geçmez — asıl güvenlik backend'de).

## Doğrulama

Uçtan uca (gerçek backend + tarayıcı) doğrulanan senaryo: 2 satırlı bir mal kabul oluşturma → listede Taslak rozetiyle görünme → detayda satırların doğru gösterilmesi → Onayla → Onaylandı'ya geçiş, Onay Tarihi'nin doğru (saat dilimi kaymasız) görünmesi ve Onayla butonunun kalkması (çifte onay engeli).
