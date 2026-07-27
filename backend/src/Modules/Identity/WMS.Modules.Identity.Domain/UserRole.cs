using WMS.SharedKernel;

namespace WMS.Modules.Identity.Domain;

/// <summary>Join entity between <see cref="User"/> and <see cref="Role"/>. Owned by the User aggregate.</summary>
public sealed class UserRole : BaseEntity
{
    private UserRole()
    {
    }

    private UserRole(Guid id, Guid userId, Guid roleId)
        : base(id)
    {
        UserId = userId;
        RoleId = roleId;
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    internal static UserRole Create(Guid userId, Guid roleId) => new(Guid.CreateVersion7(), userId, roleId);
}
