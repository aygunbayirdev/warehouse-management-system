using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Transfer.Application.Dtos;
using WMS.Modules.Transfer.Domain;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed record GetStockTransfersQuery(
    Guid? SourceWarehouseId,
    Guid? DestinationWarehouseId,
    StockTransferStatus? Status,
    int Page,
    int PageSize) : IQuery<PagedResult<StockTransferDto>>;
