using FluentAssertions;
using NSubstitute;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.Inventory.Domain;

namespace WMS.Modules.Inventory.UnitTests;

public class DecreaseStockCommandHandlerTests
{
    private readonly IStockItemWriteRepository _stockItemWriteRepository = Substitute.For<IStockItemWriteRepository>();
    private readonly IStockMovementWriteRepository _stockMovementWriteRepository = Substitute.For<IStockMovementWriteRepository>();
    private readonly IProcessedDomainEventWriteRepository _processedDomainEventWriteRepository = Substitute.For<IProcessedDomainEventWriteRepository>();
    private readonly DecreaseStockCommandHandler _handler;

    public DecreaseStockCommandHandlerTests()
    {
        _processedDomainEventWriteRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(false);
        _handler = new DecreaseStockCommandHandler(_stockItemWriteRepository, _stockMovementWriteRepository, _processedDomainEventWriteRepository);
    }

    [Fact]
    public async Task Handle_WithSufficientStock_DecreasesQuantityAndRecordsAMovement()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stockItem = StockItem.Create(warehouseId, productId);
        stockItem.Increase(15m);
        _stockItemWriteRepository.GetOrCreateAsync(warehouseId, productId, Arg.Any<CancellationToken>()).Returns(stockItem);

        var command = new DecreaseStockCommand(Guid.NewGuid(), 0, warehouseId, productId, 5m, "Goods issue approved");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        stockItem.Quantity.Should().Be(10m);
        _stockMovementWriteRepository.Received(1).Add(Arg.Is<StockMovement>(movement =>
            movement.Type == StockMovementType.Decrease && movement.Quantity == 5m));
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ReturnsConflictAndDoesNotRecordAMovement()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stockItem = StockItem.Create(warehouseId, productId);
        stockItem.Increase(3m);
        _stockItemWriteRepository.GetOrCreateAsync(warehouseId, productId, Arg.Any<CancellationToken>()).Returns(stockItem);

        var command = new DecreaseStockCommand(Guid.NewGuid(), 0, warehouseId, productId, 5m, "Goods issue approved");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("StockItem.InsufficientStock");
        stockItem.Quantity.Should().Be(3m);
        _stockMovementWriteRepository.DidNotReceive().Add(Arg.Any<StockMovement>());
    }

    [Fact]
    public async Task Handle_WhenSourceEventAlreadyProcessed_ReturnsSuccessWithoutMutatingStock()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sourceEventId = Guid.NewGuid();
        _processedDomainEventWriteRepository.ExistsAsync(sourceEventId, 0, Arg.Any<CancellationToken>()).Returns(true);

        var command = new DecreaseStockCommand(sourceEventId, 0, warehouseId, productId, 5m, "Goods issue approved");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _stockItemWriteRepository.DidNotReceive().GetOrCreateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _stockItemWriteRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
