using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Catalog.Application.Categories;

public sealed record CreateCategoryCommand(string Name) : ICommand<Guid>;
