using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Catalog.Application.UnitsOfMeasure;

public sealed record CreateUnitOfMeasureCommand(string Code, string Name) : ICommand<Guid>;
