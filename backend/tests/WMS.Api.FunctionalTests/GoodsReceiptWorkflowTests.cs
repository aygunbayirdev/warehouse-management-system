using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using WMS.Modules.Identity.Application.Dtos;
using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Api.FunctionalTests;

/// <summary>
/// Drives the Mal Kabul (Inbound) workflow entirely over real HTTP against a fresh database — the
/// same cross-module domain-event path (GoodsReceiptApprovedDomainEvent -> Inventory's
/// IncreaseStockCommand) that the frontend exercises, but here through the actual ASP.NET Core
/// pipeline (auth, MediatR, EF Core, the startup migration step) instead of a mocked handler. Since
/// this now goes through the transactional outbox (bkz. WMS.BuildingBlocks.Infrastructure.Outbox),
/// the stock update is applied by the real OutboxProcessor BackgroundService running inside this
/// WebApplicationFactory host, not inline with the approve call — this is the one test in the suite
/// that proves that background relay actually delivers, end to end, not just that it compiles.
/// </summary>
[Collection("Functional")]
public sealed class GoodsReceiptWorkflowTests
{
    private readonly HttpClient _client;

    public GoodsReceiptWorkflowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatingAndApprovingAGoodsReceipt_IncreasesStockInTheTargetWarehouse()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@wms.local",
            Password = "ChangeMe123!",
        });
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var unitOfMeasureId = await PostForIdAsync("/api/units-of-measure", new { Code = "ADET", Name = "Adet" });
        var warehouseId = await PostForIdAsync("/api/warehouses", new { Code = "TST-01", Name = "Test Depo", Address = (string?)null });
        var productId = await PostForIdAsync("/api/products", new
        {
            Sku = "SKU-FUNC-1",
            Name = "Fonksiyonel Test Ürünü",
            UnitOfMeasureId = unitOfMeasureId,
            CategoryId = (Guid?)null,
            MinStockQuantity = 0m,
        });

        var goodsReceiptId = await PostForIdAsync("/api/goods-receipts", new
        {
            WarehouseId = warehouseId,
            Lines = new[] { new { ProductId = productId, Quantity = 12m } },
        });

        var approveResponse = await _client.PostAsync($"/api/goods-receipts/{goodsReceiptId}/approve", content: null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // The approval only writes an outbox row synchronously; the stock increase itself happens
        // whenever OutboxProcessor's background polling loop next picks it up (every 5s), so this
        // polls instead of asserting immediately.
        var stockItems = await PollUntilStockMatchesAsync(warehouseId, productId, expectedQuantity: 12m);
        stockItems.Should().ContainSingle();
        stockItems![0].Quantity.Should().Be(12m);
    }

    private async Task<List<StockItemDto>?> PollUntilStockMatchesAsync(Guid warehouseId, Guid productId, decimal expectedQuantity)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        List<StockItemDto>? stockItems;

        do
        {
            var stockResponse = await _client.GetAsync($"/api/stock?warehouseId={warehouseId}&productId={productId}");
            stockResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            stockItems = await stockResponse.Content.ReadFromJsonAsync<List<StockItemDto>>();

            if (stockItems is [{ } item] && item.Quantity == expectedQuantity)
            {
                return stockItems;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250));
        } while (DateTime.UtcNow < deadline);

        return stockItems;
    }

    private async Task<Guid> PostForIdAsync(string url, object payload)
    {
        var response = await _client.PostAsJsonAsync(url, payload);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"POST {url} should succeed, but got: {body}");

        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}
