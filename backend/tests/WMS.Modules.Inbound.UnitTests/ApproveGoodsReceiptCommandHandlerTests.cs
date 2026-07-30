using FluentAssertions;
using NSubstitute;
using WMS.Modules.Inbound.Application.Abstractions;
using WMS.Modules.Inbound.Application.GoodsReceipts;
using WMS.Modules.Inbound.Domain;

namespace WMS.Modules.Inbound.UnitTests;

public class ApproveGoodsReceiptCommandHandlerTests
{
    private readonly IGoodsReceiptWriteRepository _writeRepository = Substitute.For<IGoodsReceiptWriteRepository>();
    private readonly ApproveGoodsReceiptCommandHandler _handler;

    public ApproveGoodsReceiptCommandHandlerTests()
    {
        _handler = new ApproveGoodsReceiptCommandHandler(_writeRepository);
    }

    [Fact]
    public async Task Handle_WithADraftReceiptWithLines_ApprovesItAndRaisesADomainEvent()
    {
        var goodsReceipt = GoodsReceipt.Create(Guid.NewGuid(), Guid.NewGuid());
        goodsReceipt.AddLine(Guid.NewGuid(), 10m);
        _writeRepository.GetByIdAsync(goodsReceipt.Id, Arg.Any<CancellationToken>()).Returns(goodsReceipt);

        var result = await _handler.Handle(new ApproveGoodsReceiptCommand(goodsReceipt.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        goodsReceipt.Status.Should().Be(GoodsReceiptStatus.Approved);
        goodsReceipt.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<GoodsReceiptApprovedDomainEvent>();
        await _writeRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAlreadyApproved_ReturnsConflictAndDoesNotSave()
    {
        var goodsReceipt = GoodsReceipt.Create(Guid.NewGuid(), Guid.NewGuid());
        goodsReceipt.AddLine(Guid.NewGuid(), 10m);
        goodsReceipt.Approve();
        _writeRepository.GetByIdAsync(goodsReceipt.Id, Arg.Any<CancellationToken>()).Returns(goodsReceipt);

        var result = await _handler.Handle(new ApproveGoodsReceiptCommand(goodsReceipt.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GoodsReceipt.NotDraft");
        await _writeRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoLines_ReturnsValidationFailure()
    {
        var goodsReceipt = GoodsReceipt.Create(Guid.NewGuid(), Guid.NewGuid());
        _writeRepository.GetByIdAsync(goodsReceipt.Id, Arg.Any<CancellationToken>()).Returns(goodsReceipt);

        var result = await _handler.Handle(new ApproveGoodsReceiptCommand(goodsReceipt.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GoodsReceipt.NoLines");
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var missingId = Guid.NewGuid();
        _writeRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((GoodsReceipt?)null);

        var result = await _handler.Handle(new ApproveGoodsReceiptCommand(missingId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GoodsReceipt.NotFound");
    }
}
