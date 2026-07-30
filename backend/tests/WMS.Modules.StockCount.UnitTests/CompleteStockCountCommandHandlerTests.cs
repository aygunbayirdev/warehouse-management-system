using FluentAssertions;
using NSubstitute;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Application.StockCounts;
using WMS.Modules.StockCount.Domain;
using StockCountAggregate = WMS.Modules.StockCount.Domain.StockCount;

namespace WMS.Modules.StockCount.UnitTests;

public class CompleteStockCountCommandHandlerTests
{
    private readonly IStockCountWriteRepository _stockCountWriteRepository = Substitute.For<IStockCountWriteRepository>();
    private readonly IStockCountAdjustmentWriteRepository _adjustmentWriteRepository = Substitute.For<IStockCountAdjustmentWriteRepository>();
    private readonly CompleteStockCountCommandHandler _handler;

    public CompleteStockCountCommandHandlerTests()
    {
        _handler = new CompleteStockCountCommandHandler(_stockCountWriteRepository, _adjustmentWriteRepository);
    }

    [Fact]
    public async Task Handle_CreatesAPendingAdjustmentOnlyForLinesWithANonZeroDifference()
    {
        var stockCount = StockCountAggregate.Create(Guid.NewGuid(), Guid.NewGuid());
        stockCount.Start();
        var productWithVariance = Guid.NewGuid();
        var productWithoutVariance = Guid.NewGuid();
        stockCount.AddLine(productWithVariance, systemQuantity: 15m, countedQuantity: 12m);
        stockCount.AddLine(productWithoutVariance, systemQuantity: 8m, countedQuantity: 8m);
        _stockCountWriteRepository.GetByIdAsync(stockCount.Id, Arg.Any<CancellationToken>()).Returns(stockCount);

        var result = await _handler.Handle(new CompleteStockCountCommand(stockCount.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        stockCount.Status.Should().Be(StockCountStatus.Completed);
        _adjustmentWriteRepository.Received(1).Add(Arg.Is<StockCountAdjustment>(adjustment =>
            adjustment.ProductId == productWithVariance && adjustment.DifferenceQuantity == -3m));
        _adjustmentWriteRepository.DidNotReceive().Add(Arg.Is<StockCountAdjustment>(adjustment =>
            adjustment.ProductId == productWithoutVariance));
    }

    [Fact]
    public async Task Handle_WithNoLines_ReturnsValidationFailure()
    {
        var stockCount = StockCountAggregate.Create(Guid.NewGuid(), Guid.NewGuid());
        stockCount.Start();
        _stockCountWriteRepository.GetByIdAsync(stockCount.Id, Arg.Any<CancellationToken>()).Returns(stockCount);

        var result = await _handler.Handle(new CompleteStockCountCommand(stockCount.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("StockCount.NoLines");
        _adjustmentWriteRepository.DidNotReceive().Add(Arg.Any<StockCountAdjustment>());
    }

    [Fact]
    public async Task Handle_WhenStillDraft_ReturnsConflict()
    {
        var stockCount = StockCountAggregate.Create(Guid.NewGuid(), Guid.NewGuid());
        _stockCountWriteRepository.GetByIdAsync(stockCount.Id, Arg.Any<CancellationToken>()).Returns(stockCount);

        var result = await _handler.Handle(new CompleteStockCountCommand(stockCount.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("StockCount.NotInProgress");
    }
}
