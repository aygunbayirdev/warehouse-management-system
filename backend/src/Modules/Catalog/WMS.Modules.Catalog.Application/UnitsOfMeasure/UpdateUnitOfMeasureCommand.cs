using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Catalog.Application.UnitsOfMeasure;

public sealed record UpdateUnitOfMeasureCommand(Guid Id, string Code, string Name) : ICommand;
