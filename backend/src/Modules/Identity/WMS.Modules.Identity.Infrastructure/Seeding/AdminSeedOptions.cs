namespace WMS.Modules.Identity.Infrastructure.Seeding;

public sealed class AdminSeedOptions
{
    public const string SectionName = "Identity:AdminSeed";

    public string Email { get; init; } = "admin@wms.local";

    public string Password { get; init; } = "ChangeMe123!";

    public string FirstName { get; init; } = "System";

    public string LastName { get; init; } = "Admin";
}
