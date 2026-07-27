using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Domain;

public sealed class Warehouse : BaseEntity
{
    private Warehouse()
    {
    }

    private Warehouse(Guid id, string code, string name, string? address)
        : base(id)
    {
        Code = code;
        Name = name;
        Address = address;
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Address { get; private set; }

    public static Warehouse Create(string code, string name, string? address)
    {
        Guard.AgainstNullOrWhiteSpace(code, nameof(code));
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        return new Warehouse(Guid.CreateVersion7(), code.Trim().ToUpperInvariant(), name.Trim(), address?.Trim());
    }

    public void Update(string name, string? address)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        Name = name.Trim();
        Address = address?.Trim();
    }
}
