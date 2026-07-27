using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Identity.Application.Dtos;

namespace WMS.Modules.Identity.Application.Users;

public sealed record GetCurrentUserQuery(Guid UserId) : IQuery<UserDto>;
