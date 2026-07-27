using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Application.Categories;

public sealed record GetCategoriesQuery : IQuery<IReadOnlyCollection<CategoryDto>>;
