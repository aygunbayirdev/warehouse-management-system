using FluentAssertions;
using NSubstitute;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.Inventory.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.UnitTests;

public class IncreaseStockCommandHandlerTests
{
    private readonly IStockItemWriteRepository _stockItemWriteRepository = Substitute.For<IStockItemWriteRepository>();
    private readonly IStockMovementWriteRepository _stockMovementWriteRepository = Substitute.For<IStockMovementWriteRepository>();
    private readonly IProcessedDomainEventWriteRepository _processedDomainEventWriteRepository = Substitute.For<IProcessedDomainEventWriteRepository>();
    private readonly IncreaseStockCommandHandler _handler;

    public IncreaseStockCommandHandlerTests()
    {
        _processedDomainEventWriteRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(false);
        _handler = new IncreaseStockCommandHandler(_stockItemWriteRepository, _stockMovementWriteRepository, _processedDomainEventWriteRepository);
    }

    [Fact]
    public async Task Handle_IncreasesQuantityAndRecordsAMovement()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stockItem = StockItem.Create(warehouseId, productId);
        _stockItemWriteRepository.GetOrCreateAsync(warehouseId, productId, Arg.Any<CancellationToken>()).Returns(stockItem);

        var command = new IncreaseStockCommand(Guid.NewGuid(), 0, warehouseId, productId, 10m, "Goods receipt approved");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        stockItem.Quantity.Should().Be(10m);
        _stockMovementWriteRepository.Received(1).Add(Arg.Is<StockMovement>(movement =>
            movement.Type == StockMovementType.Increase && movement.Quantity == 10m));
        _processedDomainEventWriteRepository.Received(1).Add(Arg.Is<ProcessedDomainEvent>(p =>
            p.SourceEventId == command.SourceEventId && p.LineNumber == command.LineNumber));
        await _stockItemWriteRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSaveThrowsConcurrencyConflict_ReturnsConflictResult()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stockItem = StockItem.Create(warehouseId, productId);
        _stockItemWriteRepository.GetOrCreateAsync(warehouseId, productId, Arg.Any<CancellationToken>()).Returns(stockItem);
        _stockItemWriteRepository.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("conflict", new Exception()));

        var command = new IncreaseStockCommand(Guid.NewGuid(), 0, warehouseId, productId, 10m, "Goods receipt approved");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("StockItem.ConcurrencyConflict");
    }

    [Fact]
    public async Task Handle_WhenSourceEventAlreadyProcessed_ReturnsSuccessWithoutMutatingStock()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sourceEventId = Guid.NewGuid();
        _processedDomainEventWriteRepository.ExistsAsync(sourceEventId, 0, Arg.Any<CancellationToken>()).Returns(true);

        var command = new IncreaseStockCommand(sourceEventId, 0, warehouseId, productId, 10m, "Goods receipt approved");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _stockItemWriteRepository.DidNotReceive().GetOrCreateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _stockItemWriteRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
