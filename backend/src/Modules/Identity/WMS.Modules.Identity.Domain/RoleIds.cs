namespace WMS.Modules.Identity.Domain;

/// <summary>
/// Deterministic ids for the seeded roles so EF Core seed data (<c>HasData</c>) is stable across
/// environments and re-applying migrations never generates duplicate roles.
/// </summary>
public static class RoleIds
{
    public static readonly Guid Admin = new("018f0000-0000-7000-8000-000000000001");
    public static readonly Guid WarehouseManager = new("018f0000-0000-7000-8000-000000000002");
    public static readonly Guid WarehouseSupervisor = new("018f0000-0000-7000-8000-000000000003");
    public static readonly Guid WarehouseStaff = new("018f0000-0000-7000-8000-000000000004");
}
