using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Catalog.Application.Categories;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;
