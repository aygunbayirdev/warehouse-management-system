namespace WMS.Modules.Identity.Domain;

/// <summary>
/// The fixed MVP role set. Roles are seed data, not user-manageable, so the names are constants
/// shared between seeding, JWT role claims, and [Authorize(Roles = ...)] policies.
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string WarehouseManager = "DepoMuduru";
    public const string WarehouseSupervisor = "DepoSorumlusu";
    public const string WarehouseStaff = "DepoPersoneli";

    public static readonly IReadOnlyCollection<string> All =
    [
        Admin,
        WarehouseManager,
        WarehouseSupervisor,
        WarehouseStaff,
    ];
}
