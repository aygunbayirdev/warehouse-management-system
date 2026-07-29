# Sayım (StockCount + StockCountAdjustment)

## Özet

Bir depodaki fiziksel stoğun sistemdeki kayıtlarla karşılaştırılması. Bu, projedeki en karmaşık workflow'dur çünkü **iki iç içe durum makinesi** içerir: bir sayım **oturumu** (`StockCount`: Draft→InProgress→Completed) ve oturum kapanırken farkı olan her satır için otomatik oluşan bir **düzeltme** (`StockCountAdjustment`: Pending→Approved/Rejected). Diğer üç workflow'da (Mal Kabul/Sevkiyat/Transfer) "onay = stok güncellemesi" tek adımdayken, burada "sayımı kapat" ile "stoğu düzelt" birbirinden ayrı, farklı roller tarafından yapılan iki ayrı karardır — bir depo sorumlusu sayımı kapatabilir ama farkı gerçek stoğa yansıtma yetkisi sadece bir depo müdüründedir.

## Durum Makinesi

**Sayım oturumu:**
```
Draft (Taslak) --[Start/Başlat]--> InProgress (Devam Ediyor) --[Complete/Tamamla]--> Completed (Tamamlandı)
```
Satır girişi **sadece InProgress iken** yapılabilir — Draft bir oturumda henüz sayım başlamamıştır, Completed bir oturum kapanmıştır ve değiştirilemez.

**Düzeltme (her satır için ayrı, kapanışta otomatik oluşur):**
```
Pending (Bekliyor) --[Approve/Onayla]--> Approved (Onaylandı)
Pending (Bekliyor) --[Reject/Reddet]--> Rejected (Reddedildi)
```
`StockCountAdjustment`, `StockCount`'un bir satırı (`StockCountLine`) değil, **ayrı bir aggregate root**'tur — kendi id'si, kendi onay/red akışı vardır. Bunun sebebi: bir sayım oturumu kapandığında birden fazla ürün etkilenmiş olabilir, ve her biri bağımsız olarak (farklı zamanlarda, hatta farklı kişiler tarafından) onaylanabilir/reddedilebilir olmalı — hepsini tek bir "sayımı onayla" eylemine bağlamak bu esnekliği ortadan kaldırırdı.

## Roller

| Eylem | Yetkili roller |
|---|---|
| Oturum oluşturma / başlatma / tamamlama | Admin, DepoMüdürü, DepoSorumlusu |
| Satır girişi (sayılan miktar) | **Tüm roller** |
| Düzeltme onayı / reddi | **Sadece Admin, DepoMüdürü** — `DepoSorumlusu` **hariç** |

Son satır dikkat çekici: diğer üç workflow'daki "onay" eylemleri hep Admin/DepoMüdürü/DepoSorumlusu üçlüsüne açıkken, **düzeltme onayı DepoSorumlusu'na kapalı**. Bunun mantığı: DepoSorumlusu günlük operasyonu yönetir (sayımı açar, başlatır, kapatır) ama "sistemdeki stok kaydını fiilen değiştirme" yetkisi bilinçli olarak bir üst role (DepoMüdürü) bırakılmış — CLAUDE.md §4'teki rol tanımıyla tutarlı ("DepoMüdürü: kendi deposunda tam yetki... sayım düzeltme onayı").

## Backend API

İki ayrı route grubu:

**`api/stock-counts`** (`StockCountsController.cs`)

| Method | Path | Body | Not |
|---|---|---|---|
| `GET` | `/api/stock-counts` | — | Query: `warehouseId?`, `status?` |
| `GET` | `/api/stock-counts/{id}` | — | Satırlarıyla birlikte |
| `GET` | `/api/stock-counts/variance-report` | — | Query: `warehouseId?`, `fromUtc?`, `toUtc?` — sadece Completed sayımların farklı satırlarını döndüren Dapper raporu, düzeltmenin onay durumundan bağımsız |
| `POST` | `/api/stock-counts` | `{warehouseId}` | Sadece depo — satırlar burada girilmez |
| `POST` | `/api/stock-counts/{id}/start` | — | Draft→InProgress |
| `POST` | `/api/stock-counts/{id}/lines` | `{productId, countedQuantity}` | Tek satır ekler, **tüm roller** çağırabilir |
| `POST` | `/api/stock-counts/{id}/complete` | — | InProgress→Completed, farkı olan satırlar için otomatik Pending düzeltme oluşturur |

**`api/stock-count-adjustments`** (`StockCountAdjustmentsController.cs`)

| Method | Path | Body | Not |
|---|---|---|---|
| `GET` | `/api/stock-count-adjustments` | — | Query: `warehouseId?`, `status?` |
| `GET` | `/api/stock-count-adjustments/{id}` | — | |
| `POST` | `/api/stock-count-adjustments/{id}/approve` | — | Sadece Admin/DepoMüdürü |
| `POST` | `/api/stock-count-adjustments/{id}/reject` | — | Sadece Admin/DepoMüdürü |

### Diğer üç workflow'dan farklı olan tasarım: oluşturma ≠ satır girişi

Mal Kabul/Sevkiyat/Transfer'de bir kayıt **tek bir `POST` isteğiyle tüm satırlarıyla birlikte** oluşturuluyordu (`{warehouseId, lines: [...]}`). Sayım'da bu mümkün değil, çünkü sayım fiziksel olarak zaman alan bir süreç — depo sahasında dolaşan biri ürünleri tek tek sayıp sisteme giriyor, hepsini önceden bilip tek seferde gönderemiyor. Bu yüzden `POST /api/stock-counts` sadece boş bir oturum açıyor (`{warehouseId}`), her sayılan ürün ayrı bir `POST /api/stock-counts/{id}/lines` isteğiyle ekleniyor. Bu, frontend tarafında da diğer üç workflow'un `New{Aggregate}Page` (tek seferlik çok satırlı form) deseninden ayrılıp, **`StockCountDetailPage`'in kendisinin** satır girişi arayüzü olmasını gerektiriyor — aşağıya bakınız.

### `SistemMiktarı` nasıl donuyor?

Bir satır eklendiğinde (`SubmitStockCountLineCommand`), backend o anda Inventory'nin `GetStockItemsQuery`'sini çağırıp o depodaki o ürünün **o andaki** miktarını okur ve `StockCountLine.SystemQuantity` olarak donduruyor. Bu önemli: sayım devam ederken (örneğin başka bir Mal Kabul onaylanıp) gerçek stok değişse bile, zaten girilmiş bir satırın `SistemMiktarı`'ı **geriye dönük güncellenmiyor** — o satır, o ürün sayıldığı andaki sistem durumunu yansıtıyor. `Fark = SayılanMiktar - SistemMiktarı` da bu donmuş değer üzerinden hesaplanıyor.

### Hata kodları

| Durum | `title` | HTTP |
|---|---|---|
| Draft olmayanı başlatma | `StockCount.NotDraft` | 409 |
| InProgress olmayana satır girme / InProgress olmayanı tamamlama | `StockCount.NotInProgress` | 409 |
| Aynı üründen bu sayımda ikinci kez satır girme | `StockCount.DuplicateLine` | 409 |
| Hiç satırı olmayan sayımı tamamlama | `StockCount.NoLines` | 400 |
| Pending olmayan bir düzeltmeyi onaylama/reddetme (çifte karar) | `StockCountAdjustment.NotPending` | 409 |
| Var olmayan id / depo / ürün | `*.NotFound` | 404 |

`StockCount.DuplicateLine` ve `StockCountAdjustment.NotPending` — bu ikisi diğer workflow'ların çoğu "UI zaten engelliyor" hata koduyla aynı kategoride görünse de, **gerçekten tetiklenebilir**: sayım satır girişi tüm rollere açık olduğu için aynı anda birden fazla kişi aynı sayıma satır girebilir (concürrensi), ve düzeltme onayı da birden fazla yönetici tarafından aynı anda denenebilir. Bu yüzden ikisi de `lib/errors.ts`'e Türkçe mesajla eklendi (diğer workflow'ların `NotDraft`/`NotFound` gibi salt UI-engellenen hataları için yapılmadığı gibi).

### Stok güncellemesi

`StockCountAdjustment.Approve()` çağrıldığında `StockCountAdjustmentApprovedDomainEvent` yayınlanır; bu event'i dinleyen handler (`StockCountAdjustmentApprovedDomainEventHandler`, StockCount modülünde yaşar) `DifferenceQuantity`'nin işaretine bakarak Inventory'nin `IncreaseStockCommand`'ını (pozitif fark — sayımda fazla çıkmış) veya `DecreaseStockCommand`'ını (negatif fark — sayımda eksik çıkmış) çağırır. Diğer üç workflow'daki "ayrı transaction, hata durumunda sadece log" kuralı burada da geçerli (bkz. [mal-kabul.md](mal-kabul.md#stok-güncellemesi-nasıl-oluyor)). **Reddedilen bir düzeltme hiçbir event yayınlamaz — stok hiç değişmez.**

## Frontend

`frontend/src/features/stock-count/`:

| Dosya | Görev |
|---|---|
| `types.ts` | `StockCountStatus`, `StockCountDto`, `StockCountLineDto`, `StockCountAdjustmentStatus`, `StockCountAdjustmentDto` |
| `api/stockCounts.ts` | `useStockCounts(filters)`, `useStockCount(id)`, `useCreateStockCount()`, `useStartStockCount()`, `useSubmitStockCountLine(stockCountId)`, `useCompleteStockCount()` |
| `api/stockCountAdjustments.ts` | `useStockCountAdjustments(filters)`, `useApproveStockCountAdjustment()`, `useRejectStockCountAdjustment()` |
| `StockCountsPage.tsx` | Liste (Depo/Durum filtresi, durum rozeti, satır sayısı) + "Yeni Sayım" (sadece depo seçimi içeren küçük bir `Dialog`, session-manage rollerine açık) |
| `StockCountDetailPage.tsx` | `/stock-counts/:id` — **diğer üç workflow'dan farklı olarak bir `Dialog` değil, ayrı bir route/sayfa** |
| `StockCountAdjustmentsPage.tsx` | `/stock-count-adjustments` — ayrı bir liste sayfası, satır bazında Onayla/Reddet |

### Neden `StockCountDetailPage` bir Dialog değil, ayrı bir sayfa?

Mal Kabul/Sevkiyat/Transfer'in detay görünümü basitti: satırları göster + tek bir onay/gönder/teslim-al butonu — bir `Dialog` için yeterli. Sayım'da detay görünümü **zaman içinde tekrar tekrar etkileşime giriliyor**: önce Başlat, sonra bir süre boyunca (belki dakikalar, belki saatler) tek tek ürün sayıp satır ekleniyor, en sonunda Tamamla. Bu süreci bir modal dialog içinde tutmak (kullanıcının sayfayı kapatıp geri dönebilmesi, URL ile paylaşabilmesi gereken bir iş akışı için) doğru değildi — bu yüzden ilk kez burada, diğer üç workflow'un aksine, kendi route'u olan bir detay **sayfası** (`/stock-counts/:id`) tasarlandı. İleride benzer "uzun süreli, tekrar eden etkileşim" gerektiren bir workflow eklenirse bu desen (Dialog yerine sayfa) tekrar kullanılabilir.

### Satır girişi ve tekrar sayım koruması

`StockCountDetailPage`, `InProgress` durumdayken bir satır-girişi bölümü gösterir: `ProductLookupDialog` ile ürün seçimi + miktar `Input`'u + "Satır Ekle" butonu. Diğer workflow'ların `New{Aggregate}Page`'lerindeki `excludeProductIds` deseni burada da kullanılıyor — zaten sayılmış ürünler (`stockCount.lines` üzerinden) dialogda listelenmiyor, böylece backend'in `StockCount.DuplicateLine` hatasına hiç düşülmeden aynı ürünün ikinci kez sayılması fiziksel olarak engelleniyor (uçtan uca doğrulamada teyit edildi — bkz. aşağı).

Satır ekleme, diğer workflow'ların çok-satırlı oluşturma formlarından farklı olarak **anlık bir API çağrısı** (`useSubmitStockCountLine`) — form state'inde biriktirilip tek seferde gönderilmiyor, çünkü backend'in kendisi de satırları teker teker kabul ediyor (bkz. yukarıdaki "oluşturma ≠ satır girişi" notu).

### Sayımı Tamamla

"Sayımı Tamamla" butonu, backend'in `StockCount.NoLines` hatasına erken geri bildirim olarak `stockCount.lines.length === 0` iken devre dışı bırakılıyor; aktif olduğunda bir `AlertDialog` ile onay isteniyor ("farkı olan her satır için onay bekleyen bir düzeltme kaydı otomatik oluşturulacaktır" uyarısıyla).

### Düzeltmeler sayfası

`StockCountAdjustmentsPage`, diğer workflow'ların detay-dialog deseninden farklı olarak satır bazında doğrudan tabloya gömülü Onayla/Reddet butonları kullanıyor (ayrı bir detay görünümüne gerek yok — gösterilecek tek bilgi zaten satırda: depo, ürün, fark, durum). Butonlar sadece `status === 'Pending'` olan satırlarda ve sadece `useHasAnyRole([Admin, WarehouseManager])` `true` dönerse görünüyor (backend'in `ApproveRoles`'uyla birebir aynı — DepoSorumlusu burada yok).

Route'lar: `/stock-counts`, `/stock-counts/:id`, `/stock-count-adjustments` — hepsi rol kısıtı olmadan `ProtectedRoute` altında (backend zaten her action'ı kendi rolüyle koruyor). Nav'da iki ayrı link: "Sayım" ve "Sayım Düzeltmeleri".

`lib/errors.ts`'e eklenen Türkçe mesajlar: `StockCount.DuplicateLine` → "Bu ürün bu sayımda zaten sayılmış.", `StockCountAdjustment.NotPending` → "Bu düzeltme zaten karara bağlanmış." (yukarıda açıklandığı gibi, bu ikisi gerçek concurrency senaryolarında tetiklenebildiği için diğer "UI zaten engelliyor" hatalarının aksine eklendi).

## Doğrulama

Uçtan uca (gerçek backend + tarayıcı, `admin@wms.local`) doğrulanan senaryo: Ankara Depo için yeni bir sayım oluşturma → Başlat (Devam Ediyor'a geçiş) → SKU-100'ü 12 olarak sayma (sistem miktarı 15, fark -3) → SKU-200'ü 7 olarak sayma (sistem miktarı 7.5, fark -0.5) → aynı ürünü (SKU-100) tekrar seçmeye çalışınca ürün seç penceresinde artık listelenmediğinin doğrulanması → Sayımı Tamamla (Tamamlandı'ya geçiş, iki satır için de Pending düzeltme otomatik oluştu) → Düzeltmeleri Görüntüle linkiyle düzeltmeler sayfasına geçiş → SKU-100'ün düzeltmesini Onayla (Inventory stoğunun API üzerinden 15'ten 12'ye düştüğü doğrulandı) → SKU-200'ün düzeltmesini Reddet (Inventory stoğunun 7.5'te değişmeden kaldığı doğrulandı) — hepsi konsol hatasız.
