using MediatR;
using Microsoft.Extensions.Logging;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inbound.Domain;
using WMS.Modules.Inventory.Application.StockItems;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

/// <summary>
/// Reacts to the Inbound module's own domain event by calling Inventory's public
/// IncreaseStockCommand for each line. The stock update runs in its own transaction (Inventory's
/// DbContext, separate from the already-committed GoodsReceipt approval), so a failure here cannot
/// roll back the approval — it is logged loudly instead of thrown, since by this point the approval
/// has already succeeded from the caller's perspective. See CLAUDE.md's note on this trade-off.
/// </summary>
public sealed class GoodsReceiptApprovedDomainEventHandler(
    ISender sender,
    ILogger<GoodsReceiptApprovedDomainEventHandler> logger)
    : IDomainEventHandler<GoodsReceiptApprovedDomainEvent>
{
    public async Task Handle(DomainEventNotification<GoodsReceiptApprovedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var lineNumber = 0;

        foreach (var line in domainEvent.Lines)
        {
            var command = new IncreaseStockCommand(
                notification.OutboxMessageId,
                lineNumber,
                domainEvent.WarehouseId,
                line.ProductId,
                line.Quantity,
                $"Goods receipt {domainEvent.GoodsReceiptId} approved");

            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogError(
                    "Failed to increase stock for goods receipt {GoodsReceiptId}, product {ProductId}, quantity {Quantity}: {ErrorCode} {ErrorMessage}",
                    domainEvent.GoodsReceiptId,
                    line.ProductId,
                    line.Quantity,
                    result.Error.Code,
                    result.Error.Message);
            }

            lineNumber++;
        }
    }
}
