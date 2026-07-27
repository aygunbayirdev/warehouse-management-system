using WMS.Modules.Identity.Application.Dtos;

namespace WMS.Modules.Identity.Application.Abstractions;

public interface IUserReadRepository
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
