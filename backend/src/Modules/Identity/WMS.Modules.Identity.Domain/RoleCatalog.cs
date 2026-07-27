namespace WMS.Modules.Identity.Domain;

/// <summary>
/// Maps the fixed seeded role ids to their names. Roles are not user-manageable in the MVP, so this
/// in-memory lookup avoids an extra round trip to resolve role names for JWT claims.
/// </summary>
public static class RoleCatalog
{
    public static readonly IReadOnlyDictionary<Guid, string> NamesById = new Dictionary<Guid, string>
    {
        [RoleIds.Admin] = RoleNames.Admin,
        [RoleIds.WarehouseManager] = RoleNames.WarehouseManager,
        [RoleIds.WarehouseSupervisor] = RoleNames.WarehouseSupervisor,
        [RoleIds.WarehouseStaff] = RoleNames.WarehouseStaff,
    };
}
