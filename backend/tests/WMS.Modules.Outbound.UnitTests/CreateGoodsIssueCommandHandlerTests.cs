using FluentAssertions;
using MediatR;
using NSubstitute;
using WMS.Modules.Catalog.Application.Dtos;
using WMS.Modules.Catalog.Application.Products;
using WMS.Modules.Inventory.Application.Dtos;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.Inventory.Application.Warehouses;
using WMS.Modules.Outbound.Application.Abstractions;
using WMS.Modules.Outbound.Application.GoodsIssues;
using WMS.Modules.Outbound.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Outbound.UnitTests;

public class CreateGoodsIssueCommandHandlerTests
{
    private readonly IGoodsIssueWriteRepository _writeRepository = Substitute.For<IGoodsIssueWriteRepository>();
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly CreateGoodsIssueCommandHandler _handler;

    private readonly Guid _warehouseId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public CreateGoodsIssueCommandHandlerTests()
    {
        _handler = new CreateGoodsIssueCommandHandler(_writeRepository, _sender);

        _sender.Send(Arg.Any<GetWarehouseByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new WarehouseDto(_warehouseId, "ANK-01", "Ankara Depo", null)));
        _sender.Send(Arg.Any<GetProductByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new ProductDto(_productId, "SKU-100", "Test Ürün", Guid.NewGuid(), "ADET", null, null, 0m)));
    }

    private void StubAvailableQuantity(decimal quantity)
    {
        _sender.Send(Arg.Any<GetStockItemsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyCollection<StockItemDto>>(
            [
                new StockItemDto(_warehouseId, "ANK-01", "Ankara Depo", _productId, "SKU-100", "Test Ürün", "ADET", quantity),
            ]));
    }

    [Fact]
    public async Task Handle_WithSufficientStock_CreatesTheGoodsIssue()
    {
        StubAvailableQuantity(15m);

        var command = new CreateGoodsIssueCommand(
            _warehouseId,
            "Müşteri A",
            [new GoodsIssueLineInput(_productId, 5m)],
            Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _writeRepository.Received(1).Add(Arg.Is<GoodsIssue>(issue => issue.Lines.Count == 1));
        await _writeRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ReturnsConflictAndDoesNotCreate()
    {
        StubAvailableQuantity(3m);

        var command = new CreateGoodsIssueCommand(
            _warehouseId,
            "Müşteri A",
            [new GoodsIssueLineInput(_productId, 5m)],
            Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GoodsIssue.InsufficientStock");
        _writeRepository.DidNotReceive().Add(Arg.Any<GoodsIssue>());
    }
}
