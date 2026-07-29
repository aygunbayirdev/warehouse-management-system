namespace WMS.BuildingBlocks.Application.Models;

public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int TotalCount, int Page, int PageSize);
