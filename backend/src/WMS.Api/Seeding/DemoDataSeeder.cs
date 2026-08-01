using MediatR;
using Microsoft.Extensions.Options;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Categories;
using WMS.Modules.Catalog.Application.Products;
using WMS.Modules.Catalog.Application.UnitsOfMeasure;
using WMS.Modules.Identity.Application.Abstractions;
using WMS.Modules.Identity.Infrastructure.Seeding;
using WMS.Modules.Inbound.Application.GoodsReceipts;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.Inventory.Application.Warehouses;
using WMS.Modules.Outbound.Application.GoodsIssues;
using WMS.Modules.StockCount.Application.StockCounts;
using WMS.Modules.Transfer.Application.StockTransfers;
using WMS.SharedKernel;

namespace WMS.Api.Seeding;

/// <summary>
/// Populates a brand-new deployment with a small, internally consistent demo dataset (portfolio/demo
/// purposes - see TASKS.md Faz 15), so a reviewer sees a populated, working system instead of an
/// empty shell they'd have to fill in by hand. Every quantity below flows through the same
/// command handlers a real user's API call would hit (GoodsReceipt.Approve(), IncreaseStockCommand,
/// etc.), so stock levels, the movement ledger, and stock-count variances are guaranteed to agree
/// with each other - there is no hand-computed number that could drift from what the domain logic
/// actually produces.
///
/// Idempotent and non-destructive: bails out immediately if any warehouse already exists, whether
/// that's this seeder having already run once, or a real deployment with real data. Toggle via
/// appsettings' Seeding:SeedDemoData (default true).
/// </summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(IServiceProvider rootServices, CancellationToken cancellationToken = default)
    {
        using var scope = rootServices.CreateScope();
        var services = scope.ServiceProvider;
        var sender = services.GetRequiredService<ISender>();

        var existingWarehouses = await sender.Send(new GetWarehousesQuery(), cancellationToken);
        if (existingWarehouses.Value.Count > 0)
        {
            return;
        }

        var adminOptions = services.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
        var userWriteRepository = services.GetRequiredService<IUserWriteRepository>();
        var admin = await userWriteRepository.GetByEmailAsync(adminOptions.Email, cancellationToken)
            ?? throw new InvalidOperationException("Demo data seeding requires the Admin user to already be seeded.");
        var adminId = admin.Id;

        // --- Reference data -------------------------------------------------
        var adet = await SendForIdAsync(sender, new CreateUnitOfMeasureCommand("ADET", "Adet"), cancellationToken);
        var kg = await SendForIdAsync(sender, new CreateUnitOfMeasureCommand("KG", "Kilogram"), cancellationToken);
        var koli = await SendForIdAsync(sender, new CreateUnitOfMeasureCommand("KOLI", "Koli"), cancellationToken);

        var elektronik = await SendForIdAsync(sender, new CreateCategoryCommand("Elektronik"), cancellationToken);
        var gida = await SendForIdAsync(sender, new CreateCategoryCommand("Gıda"), cancellationToken);
        var ofis = await SendForIdAsync(sender, new CreateCategoryCommand("Ofis Malzemesi"), cancellationToken);
        var temizlik = await SendForIdAsync(sender, new CreateCategoryCommand("Temizlik"), cancellationToken);

        var mouse = await SendForIdAsync(sender, new CreateProductCommand("ELK-001", "Kablosuz Mouse", adet, elektronik, 10m), cancellationToken);
        var sarjKablosu = await SendForIdAsync(sender, new CreateProductCommand("ELK-002", "USB-C Şarj Kablosu", adet, elektronik, 20m), cancellationToken);
        var kulaklik = await SendForIdAsync(sender, new CreateProductCommand("ELK-003", "Bluetooth Kulaklık", adet, elektronik, 5m), cancellationToken);
        var kahve = await SendForIdAsync(sender, new CreateProductCommand("GID-001", "Filtre Kahve (1kg Paket)", kg, gida, 15m), cancellationToken);
        var cay = await SendForIdAsync(sender, new CreateProductCommand("GID-002", "Bitkisel Çay Kutusu", koli, gida, 8m), cancellationToken);
        var fotokopiKagidi = await SendForIdAsync(sender, new CreateProductCommand("OFS-001", "A4 Fotokopi Kağıdı", koli, ofis, 25m), cancellationToken);
        var kalem = await SendForIdAsync(sender, new CreateProductCommand("OFS-002", "Tükenmez Kalem", adet, ofis, 50m), cancellationToken);
        var temizleyici = await SendForIdAsync(sender, new CreateProductCommand("TMZ-001", "Yüzey Temizleyici", adet, temizlik, 12m), cancellationToken);

        var istanbul = await SendForIdAsync(sender, new CreateWarehouseCommand("IST-01", "İstanbul Ana Depo", "İkitelli OSB, İstanbul"), cancellationToken);
        var ankara = await SendForIdAsync(sender, new CreateWarehouseCommand("ANK-01", "Ankara Depo", "Ostim OSB, Ankara"), cancellationToken);

        // --- Mal Kabul: başlangıç stoğu --------------------------------------
        await CreateAndApproveGoodsReceiptAsync(sender, istanbul, adminId,
            [(mouse, 50m), (sarjKablosu, 80m), (kulaklik, 20m), (kahve, 40m), (cay, 30m)], cancellationToken);
        await CreateAndApproveGoodsReceiptAsync(sender, istanbul, adminId,
            [(fotokopiKagidi, 60m), (kalem, 100m), (temizleyici, 25m)], cancellationToken);
        await CreateAndApproveGoodsReceiptAsync(sender, ankara, adminId,
            [(mouse, 30m), (sarjKablosu, 40m), (kahve, 20m), (fotokopiKagidi, 35m)], cancellationToken);

        // --- Sevkiyat: müşteriye çıkış ----------------------------------------
        await CreateAndApproveGoodsIssueAsync(sender, istanbul, "Acme Elektronik A.Ş.", adminId,
            [(mouse, 8m), (sarjKablosu, 15m)], cancellationToken);
        await CreateAndApproveGoodsIssueAsync(sender, ankara, "Baştaş Ticaret", adminId,
            [(kahve, 5m), (fotokopiKagidi, 10m)], cancellationToken);

        // --- Transfer: biri tamamlanmış, biri taslak (canlı denemek için) -----
        var completedTransferId = await SendForIdAsync(
            sender,
            new CreateStockTransferCommand(istanbul, ankara, [new StockTransferLineInput(kulaklik, 10m)], adminId),
            cancellationToken);
        await SendAsync(sender, new ShipStockTransferCommand(completedTransferId), cancellationToken);
        await SendAsync(sender, new ReceiveStockTransferCommand(completedTransferId), cancellationToken);

        await SendForIdAsync(
            sender,
            new CreateStockTransferCommand(ankara, istanbul, [new StockTransferLineInput(mouse, 5m)], adminId),
            cancellationToken);

        // --- Sayım: biri tamamlanmış (kasıtlı küçük bir farkla, onay bekleyen
        // bir düzeltme oluşturur), biri taslak (canlı denemek için) -----------
        var completedCountId = await SendForIdAsync(sender, new CreateStockCountCommand(istanbul, adminId), cancellationToken);
        await SendAsync(sender, new StartStockCountCommand(completedCountId), cancellationToken);
        await SendAsync(sender, new SubmitStockCountLineCommand(completedCountId, mouse, 40m), cancellationToken); // sistem 42, sayılan 40 -> fark -2
        await SendAsync(sender, new SubmitStockCountLineCommand(completedCountId, cay, 30m), cancellationToken); // sistem 30, sayılan 30 -> fark yok
        await SendAsync(sender, new CompleteStockCountCommand(completedCountId), cancellationToken);

        await SendForIdAsync(sender, new CreateStockCountCommand(ankara, adminId), cancellationToken);
    }

    private static async Task CreateAndApproveGoodsReceiptAsync(
        ISender sender,
        Guid warehouseId,
        Guid createdByUserId,
        (Guid ProductId, decimal Quantity)[] lines,
        CancellationToken cancellationToken)
    {
        var before = await GetQuantitiesAsync(sender, warehouseId, lines.Select(line => line.ProductId), cancellationToken);

        var lineInputs = lines.Select(line => new GoodsReceiptLineInput(line.ProductId, line.Quantity)).ToList();
        var id = await SendForIdAsync(sender, new CreateGoodsReceiptCommand(warehouseId, lineInputs, createdByUserId), cancellationToken);
        await SendAsync(sender, new ApproveGoodsReceiptCommand(id), cancellationToken);

        // Approval only raises a domain event now (bkz. WMS.BuildingBlocks.Infrastructure.Outbox) —
        // Inventory's stock increase happens asynchronously via the OutboxProcessor relay, not inline
        // with this call. Later seeding steps (a goods issue, a stock count) assume this stock is
        // already there, so we wait for it here rather than pushing that assumption onto every caller.
        var expected = lines.Select(line => (line.ProductId, before[line.ProductId] + line.Quantity));
        await WaitForStockAsync(sender, warehouseId, expected, cancellationToken);
    }

    private static async Task CreateAndApproveGoodsIssueAsync(
        ISender sender,
        Guid warehouseId,
        string destination,
        Guid createdByUserId,
        (Guid ProductId, decimal Quantity)[] lines,
        CancellationToken cancellationToken)
    {
        var before = await GetQuantitiesAsync(sender, warehouseId, lines.Select(line => line.ProductId), cancellationToken);

        var lineInputs = lines.Select(line => new GoodsIssueLineInput(line.ProductId, line.Quantity)).ToList();
        var id = await SendForIdAsync(sender, new CreateGoodsIssueCommand(warehouseId, destination, lineInputs, createdByUserId), cancellationToken);
        await SendAsync(sender, new ApproveGoodsIssueCommand(id), cancellationToken);

        var expected = lines.Select(line => (line.ProductId, before[line.ProductId] - line.Quantity));
        await WaitForStockAsync(sender, warehouseId, expected, cancellationToken);
    }

    private static async Task<Dictionary<Guid, decimal>> GetQuantitiesAsync(
        ISender sender, Guid warehouseId, IEnumerable<Guid> productIds, CancellationToken cancellationToken)
    {
        var stockItems = await sender.Send(new GetStockItemsQuery(warehouseId, null), cancellationToken);

        return productIds.ToDictionary(
            productId => productId,
            productId => stockItems.Value.FirstOrDefault(item => item.ProductId == productId)?.Quantity ?? 0m);
    }

    private static async Task WaitForStockAsync(
        ISender sender,
        Guid warehouseId,
        IEnumerable<(Guid ProductId, decimal ExpectedQuantity)> expectations,
        CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(20);

        foreach (var (productId, expectedQuantity) in expectations)
        {
            while (true)
            {
                var stockItems = await sender.Send(new GetStockItemsQuery(warehouseId, productId), cancellationToken);
                var actualQuantity = stockItems.Value.FirstOrDefault()?.Quantity ?? 0m;

                if (actualQuantity == expectedQuantity)
                {
                    break;
                }

                if (DateTime.UtcNow > deadline)
                {
                    throw new InvalidOperationException(
                        $"Timed out waiting for the outbox relay to apply a stock update (warehouse {warehouseId}, product {productId}, expected {expectedQuantity}, got {actualQuantity}).");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
            }
        }
    }

    private static async Task<Guid> SendForIdAsync(ISender sender, ICommand<Guid> command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException($"Demo data seeding failed for {command.GetType().Name}: {result.Error.Code} - {result.Error.Message}");
    }

    private static async Task SendAsync(ISender sender, ICommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Demo data seeding failed for {command.GetType().Name}: {result.Error.Code} - {result.Error.Message}");
        }
    }
}
