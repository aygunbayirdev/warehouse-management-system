using FluentAssertions;
using MediatR;
using NSubstitute;
using WMS.Modules.Inventory.Application.Dtos;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.Transfer.Application.Abstractions;
using WMS.Modules.Transfer.Application.StockTransfers;
using WMS.Modules.Transfer.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Transfer.UnitTests;

public class ShipStockTransferCommandHandlerTests
{
    private readonly IStockTransferWriteRepository _writeRepository = Substitute.For<IStockTransferWriteRepository>();
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly ShipStockTransferCommandHandler _handler;

    public ShipStockTransferCommandHandlerTests()
    {
        _handler = new ShipStockTransferCommandHandler(_writeRepository, _sender);
    }

    private void StubAvailableQuantity(Guid warehouseId, Guid productId, decimal quantity)
    {
        _sender.Send(Arg.Any<GetStockItemsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyCollection<StockItemDto>>(
            [
                new StockItemDto(warehouseId, "ANK-01", "Ankara Depo", productId, "SKU-100", "Test Ürün", "ADET", quantity),
            ]));
    }

    [Fact]
    public async Task Handle_WithSufficientSourceStock_ShipsAndRaisesADomainEvent()
    {
        var productId = Guid.NewGuid();
        var createResult = StockTransfer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var stockTransfer = createResult.Value;
        stockTransfer.AddLine(productId, 5m);
        StubAvailableQuantity(stockTransfer.SourceWarehouseId, productId, 10m);
        _writeRepository.GetByIdAsync(stockTransfer.Id, Arg.Any<CancellationToken>()).Returns(stockTransfer);

        var result = await _handler.Handle(new ShipStockTransferCommand(stockTransfer.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        stockTransfer.Status.Should().Be(StockTransferStatus.Shipped);
        stockTransfer.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<StockTransferShippedDomainEvent>();
        await _writeRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInsufficientSourceStock_ReturnsConflictAndDoesNotShip()
    {
        var productId = Guid.NewGuid();
        var createResult = StockTransfer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var stockTransfer = createResult.Value;
        stockTransfer.AddLine(productId, 5m);
        StubAvailableQuantity(stockTransfer.SourceWarehouseId, productId, 3m);
        _writeRepository.GetByIdAsync(stockTransfer.Id, Arg.Any<CancellationToken>()).Returns(stockTransfer);

        var result = await _handler.Handle(new ShipStockTransferCommand(stockTransfer.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("StockTransfer.InsufficientStock");
        stockTransfer.Status.Should().Be(StockTransferStatus.Draft);
        await _writeRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
