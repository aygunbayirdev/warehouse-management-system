using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Domain;

public sealed class Category : BaseEntity
{
    private Category()
    {
    }

    private Category(Guid id, string name)
        : base(id)
    {
        Name = name;
    }

    public string Name { get; private set; } = string.Empty;

    public static Category Create(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        return new Category(Guid.CreateVersion7(), name.Trim());
    }

    public void Update(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        Name = name.Trim();
    }
}
