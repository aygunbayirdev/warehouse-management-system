using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Transfer.Application.Dtos;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed record GetStockTransferByIdQuery(Guid Id) : IQuery<StockTransferDto>;
