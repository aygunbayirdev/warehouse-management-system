using MediatR;
using Microsoft.Extensions.Logging;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

/// <summary>
/// Reacts to the StockCount module's own domain event by calling Inventory's public
/// IncreaseStockCommand or DecreaseStockCommand (chosen by the sign of DifferenceQuantity). The
/// stock update runs in its own transaction (Inventory's DbContext, separate from the
/// already-committed adjustment approval), so a failure here cannot roll back the approval — it is
/// logged loudly instead of thrown. See CLAUDE.md's note on this trade-off.
/// </summary>
public sealed class StockCountAdjustmentApprovedDomainEventHandler(
    ISender sender,
    ILogger<StockCountAdjustmentApprovedDomainEventHandler> logger)
    : IDomainEventHandler<StockCountAdjustmentApprovedDomainEvent>
{
    public async Task Handle(DomainEventNotification<StockCountAdjustmentApprovedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var reason = $"Stock count adjustment {domainEvent.StockCountAdjustmentId} approved";

        const int lineNumber = 0;

        var result = domainEvent.DifferenceQuantity > 0
            ? await sender.Send(
                new IncreaseStockCommand(
                    notification.OutboxMessageId, lineNumber, domainEvent.WarehouseId, domainEvent.ProductId, domainEvent.DifferenceQuantity, reason),
                cancellationToken)
            : await sender.Send(
                new DecreaseStockCommand(
                    notification.OutboxMessageId, lineNumber, domainEvent.WarehouseId, domainEvent.ProductId, -domainEvent.DifferenceQuantity, reason),
                cancellationToken);

        if (result.IsFailure)
        {
            logger.LogError(
                "Failed to apply stock count adjustment {StockCountAdjustmentId}, product {ProductId}, difference {DifferenceQuantity}: {ErrorCode} {ErrorMessage}",
                domainEvent.StockCountAdjustmentId,
                domainEvent.ProductId,
                domainEvent.DifferenceQuantity,
                result.Error.Code,
                result.Error.Message);
        }
    }
}
