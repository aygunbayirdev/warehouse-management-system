using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Catalog.Application.UnitsOfMeasure;

public sealed record DeleteUnitOfMeasureCommand(Guid Id) : ICommand;
