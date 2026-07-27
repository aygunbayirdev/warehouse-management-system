using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Application.UnitsOfMeasure;

public sealed record GetUnitOfMeasureByIdQuery(Guid Id) : IQuery<UnitOfMeasureDto>;
