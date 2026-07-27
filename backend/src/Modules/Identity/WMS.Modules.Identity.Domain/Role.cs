using WMS.SharedKernel;

namespace WMS.Modules.Identity.Domain;

public sealed class Role : BaseEntity
{
    private Role()
    {
    }

    private Role(Guid id, string name)
        : base(id)
    {
        Name = name;
    }

    public string Name { get; private set; } = string.Empty;

    public static Role Create(Guid id, string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        return new Role(id, name);
    }
}
