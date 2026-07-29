# Depolar Arası Transfer (Transfer / StockTransfer)

## Özet

Aynı ürünün bir depodan başka bir depoya taşınması (ör. Ankara Depo → İstanbul Ana Depo). Mal Kabul/Sevkiyat'ın tek onay adımlı modelinden farklı olarak burada **iki ayrı fiziksel olay** var: mal kaynak depodan çıkar (**Gönder**) ve bir süre sonra hedef depoya girer (**Teslim Al**). Bu iki olay arasında mal "yolda" kabul edilir — kaynaktan düşülmüş ama hedefe henüz eklenmemiştir. Bu, gerçek bir kamyonla nakliyenin doğasını yansıtan bilinçli bir tasarım: aradaki boşlukta kayıp/fire senaryosuna açık bırakılmıştır (MVP kapsamında bu senaryo için ayrı bir düzeltme akışı yoktur).

## Durum Makinesi

```
Draft (Taslak) --[Ship/Gönder]--> Shipped (Gönderildi) --[Receive/Teslim Al]--> Received (Teslim Alındı)
```

Şemada bir de `Cancelled` durumu tanımlı, ama **hiçbir command bu geçişi tetiklemiyor** — yani veritabanı seviyesinde yer ayrılmış ama backend'de veya frontend'de kullanılabilir bir "iptal et" eylemi yok. Bunu ileride eklerken şemayı değiştirmeye gerek olmayacağını bilerek okuyun; bugün için sadece üç durum fiilen kullanılıyor.

## Roller

| Eylem | Yetkili roller |
|---|---|
| Oluşturma (taslak + satırlar) | **Tüm roller** |
| Gönder | Admin, DepoMüdürü, DepoSorumlusu |
| Teslim Al | Admin, DepoMüdürü, DepoSorumlusu (**Gönder ile aynı grup**) |

Mal Kabul/Sevkiyat'tan farklı olarak burada **tek bir rol grubu** (`ShipReceiveRoles`) hem Gönder hem Teslim Al eylemini yapabilir — iki farklı rol grubu yok (örn. kaynak depo sorumlusu gönderir, hedef depo sorumlusu teslim alır gibi bir ayrım MVP'de **yok**; her ikisi de aynı üç rolün elinde).

## Backend API

Route base: `api/stock-transfers` (`backend/src/WMS.Api/Controllers/StockTransfersController.cs`)

| Method | Path | Body | Not |
|---|---|---|---|
| `GET` | `/api/stock-transfers` | — | Query: `sourceWarehouseId?`, `destinationWarehouseId?`, `status?` — **iki ayrı bağımsız depo filtresi** |
| `GET` | `/api/stock-transfers/{id}` | — | |
| `POST` | `/api/stock-transfers` | `{sourceWarehouseId, destinationWarehouseId, lines:[{productId,quantity}]}` | kaynak ≠ hedef zorunlu |
| `POST` | `/api/stock-transfers/{id}/ship` | — (boş) | Kaynak depo stok kontrolü burada yapılır |
| `POST` | `/api/stock-transfers/{id}/receive` | — (boş) | Stok kontrolü yok (mal zaten "yolda" sayılıyor) |

`StockTransferDto`, Mal Kabul'ün `GoodsReceiptDto`'suna ek olarak iki depo alanı ve iki tarih alanı içerir: `sourceWarehouseId/Name`, `destinationWarehouseId/Name`, `shippedAtUtc`, `receivedAtUtc` (ikisi de `Draft` iken `null`).

### Hata kodları

| Durum | `title` | HTTP |
|---|---|---|
| Kaynak = Hedef depo | `StockTransfer.SameWarehouse` | 400 |
| **Gönderim anında** kaynak depoda yeterli stok yok | `StockTransfer.InsufficientStock` | 409 |
| Taslak olmayan bir kaydı gönderme (çifte gönderim) | `StockTransfer.NotDraft` | 409 |
| Gönderilmemiş/zaten teslim alınmış bir kaydı teslim alma | `StockTransfer.NotShipped` | 409 |
| Var olmayan id / depo / ürün | `*.NotFound` | 404 |

### ⚠️ Bilinen kısıt: aynı satır-bazlı, toplanmamış stok kontrolü

Sevkiyat'taki ile **birebir aynı** sınırlama, sadece kontrolün zamanı farklı: Sevkiyat'ta oluşturma anında kontrol edilirken, Transfer'de **gönderim (`ship`) anında** kontrol ediliyor — çünkü taslak oluşturulduğunda depo stoğu henüz düşülmüş değil, gerçek "commitment" gönderim anında oluşuyor. Ama altta yatan hata deseni aynı: aynı ürünü içeren iki satır varsa miktarlar toplanmadan her satır bağımsız kontrol ediliyor, yani teorik olarak toplam talep depodaki miktarı aşsa bile her satır tek başına yeterliymiş gibi görünüp kabul edilebilir.

Frontend telafisi de Sevkiyat'takiyle birebir aynı: `NewStockTransferPage.tsx`, `ProductLookupDialog`'a `excludeProductIds` geçirerek aynı ürünün ikinci bir satırda seçilmesini fiziksel olarak engelliyor + submit öncesi `hasDuplicateProduct` son kontrolü. Ayrıntılı gerekçe için [sevkiyat.md](sevkiyat.md#️-bilinen-kısıt-satır-bazlı-toplanmamış-stok-kontrolü)'a bakınız — burada tekrar edilmiyor.

### Stok güncellemesi — iki ayrı domain event

Bu workflow, tek bir onay yerine **iki bağımsız domain event** yayınlıyor, ikisi de Transfer modülünün kendi içinde yaşayan handler'lara sahip:

- Gönderimde: `StockTransferShippedDomainEvent` → `StockTransferShippedDomainEventHandler` → Inventory'nin `DecreaseStockCommand`'ı (kaynak depo).
- Teslim almada: `StockTransferReceivedDomainEvent` → `StockTransferReceivedDomainEventHandler` → Inventory'nin `IncreaseStockCommand`'ı (hedef depo).

İkisi de aynı "ayrı transaction, hata durumunda sadece log" kuralına tabi (bkz. [mal-kabul.md](mal-kabul.md#stok-güncellemesi-nasıl-oluyor)). Pratik sonucu: gönderim onaylandıktan sonra Inventory tarafı bir sebeple (örn. concurrency çakışması) başarısız olursa, transfer `Shipped` durumuna geçmiş olur ama kaynak depo stoğu düşmemiş olabilir — nadir görülen ama bilinen bir eventual-consistency riski, MVP'de manuel mutabakat gerektirir.

## Frontend

`frontend/src/features/transfer/`:

| Dosya | Görev |
|---|---|
| `types.ts` | `StockTransferStatus` (`Draft`/`Shipped`/`Received`/`Cancelled`), `StockTransferDto`, `CreateStockTransferPayload` |
| `api/stockTransfers.ts` | `useStockTransfers(filters)`, `useCreateStockTransfer()`, `useShipStockTransfer()`, `useReceiveStockTransfer()` |
| `StockTransfersPage.tsx` | Liste (Kaynak Depo + Hedef Depo + Durum — 3 bağımsız filtre) + detay dialog (iki depo, üç tarih, satırlar, duruma göre tek eylem butonu) |
| `NewStockTransferPage.tsx` | Kaynak/Hedef depo seçimi + satırlar |

`NewStockTransferPage.tsx`'e özgü iki ek istemci-taraflı doğrulama (backend'e gitmeden önce erken geri bildirim için):
- Kaynak ve hedef aynı depo seçilirse inline uyarı + submit butonu devre dışı (backend'in `SameWarehouse` 400'üne hiç düşmeden engellenir).
- Yukarıda anlatılan aynı-ürün-iki-satır koruması.

Detay dialogundaki eylem butonu duruma göre değişir: `Draft` ise **Gönder** (`AlertDialog` metni: "kaynak depodan düşülecek"), `Shipped` ise **Teslim Al** ("hedef depoya eklenecek") — ikisi de `useHasAnyRole([Admin, WarehouseManager, WarehouseSupervisor])` arkasında (backend'in `ShipReceiveRoles`'uyla birebir aynı).

Route'lar: `/stock-transfers`, `/stock-transfers/new` — rol kısıtı yok. Nav'da "Transfer" linki.

`lib/errors.ts`'e eklenen Türkçe mesajlar: `StockTransfer.SameWarehouse` → "Kaynak ve hedef depo aynı olamaz.", `StockTransfer.InsufficientStock` → "Depoda yeterli stok bulunmuyor." (Sevkiyat'takiyle aynı metin, farklı bağlamda).

## Doğrulama

Uçtan uca doğrulanan senaryolar:
1. Ankara → İstanbul arası, mevcut stoklu bir ürün için küçük miktarlı transfer oluşturma.
2. Aynı depoyu kaynak+hedef olarak seçmeyi deneme → inline uyarı ve devre dışı submit'in doğrulanması.
3. Aynı ürünü iki satırda seçmeyi deneme → ikinci satırda listelenmediğinin doğrulanması.
4. Oluşturma → **Gönder** → kaynak depo stoğunun azaldığının teyidi, durumun "Gönderildi"ye geçtiğinin ve Gönderim Tarihi'nin göründüğünün doğrulanması.
5. **Teslim Al** → hedef depo stoğunun arttığının teyidi, durumun "Teslim Alındı"ya geçtiğinin ve Teslim Tarihi'nin göründüğünün doğrulanması.
6. Kaynak depodaki mevcuttan fazla miktarla gönderme denemesi → `409 StockTransfer.InsufficientStock` → Türkçe mesajın doğru gösterilmesi.
