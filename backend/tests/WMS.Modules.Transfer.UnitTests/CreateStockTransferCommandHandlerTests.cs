using FluentAssertions;
using MediatR;
using NSubstitute;
using WMS.Modules.Inventory.Application.Dtos;
using WMS.Modules.Inventory.Application.Warehouses;
using WMS.Modules.Transfer.Application.Abstractions;
using WMS.Modules.Transfer.Application.StockTransfers;
using WMS.Modules.Transfer.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Transfer.UnitTests;

public class CreateStockTransferCommandHandlerTests
{
    private readonly IStockTransferWriteRepository _writeRepository = Substitute.For<IStockTransferWriteRepository>();
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly CreateStockTransferCommandHandler _handler;

    public CreateStockTransferCommandHandlerTests()
    {
        _handler = new CreateStockTransferCommandHandler(_writeRepository, _sender);
    }

    private void StubWarehouse(Guid id)
    {
        _sender.Send(Arg.Is<GetWarehouseByIdQuery>(query => query.Id == id), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new WarehouseDto(id, "CODE", "Depo", null)));
    }

    [Fact]
    public async Task Handle_WithSameSourceAndDestinationWarehouse_ReturnsValidationFailureAndDoesNotCreate()
    {
        var warehouseId = Guid.NewGuid();
        StubWarehouse(warehouseId);

        var command = new CreateStockTransferCommand(warehouseId, warehouseId, [], Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("StockTransfer.SameWarehouse");
        _writeRepository.DidNotReceive().Add(Arg.Any<StockTransfer>());
    }

    [Fact]
    public async Task Handle_WithDifferentWarehouses_CreatesTheTransfer()
    {
        var sourceId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();
        StubWarehouse(sourceId);
        StubWarehouse(destinationId);

        var command = new CreateStockTransferCommand(sourceId, destinationId, [], Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _writeRepository.Received(1).Add(Arg.Any<StockTransfer>());
        await _writeRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
