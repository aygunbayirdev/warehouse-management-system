using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Domain;

public sealed class UnitOfMeasure : BaseEntity
{
    private UnitOfMeasure()
    {
    }

    private UnitOfMeasure(Guid id, string code, string name)
        : base(id)
    {
        Code = code;
        Name = name;
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public static UnitOfMeasure Create(string code, string name)
    {
        Guard.AgainstNullOrWhiteSpace(code, nameof(code));
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        return new UnitOfMeasure(Guid.CreateVersion7(), code.Trim().ToUpperInvariant(), name.Trim());
    }

    public void Update(string code, string name)
    {
        Guard.AgainstNullOrWhiteSpace(code, nameof(code));
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
    }
}
