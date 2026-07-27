using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Identity.Application.Abstractions;
using WMS.Modules.Identity.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Identity.Application.Users;

public sealed class GetCurrentUserQueryHandler(IUserReadRepository userReadRepository)
    : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userReadRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(Error.NotFound("User.NotFound", "The user was not found."));
        }

        return user;
    }
}
