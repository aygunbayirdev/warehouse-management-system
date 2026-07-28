using MediatR;
using Microsoft.Extensions.Logging;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.Outbound.Domain;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

/// <summary>
/// Reacts to the Outbound module's own domain event by calling Inventory's public
/// DecreaseStockCommand for each line. The stock update runs in its own transaction (Inventory's
/// DbContext, separate from the already-committed GoodsIssue approval), so a failure here cannot
/// roll back the approval — it is logged loudly instead of thrown, since by this point the approval
/// has already succeeded from the caller's perspective. See CLAUDE.md's note on this trade-off.
/// </summary>
public sealed class GoodsIssueApprovedDomainEventHandler(
    ISender sender,
    ILogger<GoodsIssueApprovedDomainEventHandler> logger)
    : IDomainEventHandler<GoodsIssueApprovedDomainEvent>
{
    public async Task Handle(DomainEventNotification<GoodsIssueApprovedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        foreach (var line in domainEvent.Lines)
        {
            var command = new DecreaseStockCommand(
                domainEvent.WarehouseId,
                line.ProductId,
                line.Quantity,
                $"Goods issue {domainEvent.GoodsIssueId} approved");

            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogError(
                    "Failed to decrease stock for goods issue {GoodsIssueId}, product {ProductId}, quantity {Quantity}: {ErrorCode} {ErrorMessage}",
                    domainEvent.GoodsIssueId,
                    line.ProductId,
                    line.Quantity,
                    result.Error.Code,
                    result.Error.Message);
            }
        }
    }
}
