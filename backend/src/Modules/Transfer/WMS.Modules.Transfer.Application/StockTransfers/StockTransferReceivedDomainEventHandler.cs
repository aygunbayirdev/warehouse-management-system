using MediatR;
using Microsoft.Extensions.Logging;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.Transfer.Domain;

namespace WMS.Modules.Transfer.Application.StockTransfers;

/// <summary>
/// Reacts to the Transfer module's own domain event by calling Inventory's public
/// IncreaseStockCommand for each line. The stock update runs in its own transaction (Inventory's
/// DbContext, separate from the already-committed receipt), so a failure here cannot roll back the
/// receipt — it is logged loudly instead of thrown. See CLAUDE.md's note on this trade-off.
/// </summary>
public sealed class StockTransferReceivedDomainEventHandler(
    ISender sender,
    ILogger<StockTransferReceivedDomainEventHandler> logger)
    : IDomainEventHandler<StockTransferReceivedDomainEvent>
{
    public async Task Handle(DomainEventNotification<StockTransferReceivedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        foreach (var line in domainEvent.Lines)
        {
            var command = new IncreaseStockCommand(
                domainEvent.DestinationWarehouseId,
                line.ProductId,
                line.Quantity,
                $"Stock transfer {domainEvent.StockTransferId} received");

            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogError(
                    "Failed to increase stock for stock transfer {StockTransferId}, product {ProductId}, quantity {Quantity}: {ErrorCode} {ErrorMessage}",
                    domainEvent.StockTransferId,
                    line.ProductId,
                    line.Quantity,
                    result.Error.Code,
                    result.Error.Message);
            }
        }
    }
}
