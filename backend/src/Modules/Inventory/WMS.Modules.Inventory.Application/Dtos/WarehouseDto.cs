namespace WMS.Modules.Inventory.Application.Dtos;

public sealed record WarehouseDto(Guid Id, string Code, string Name, string? Address);
