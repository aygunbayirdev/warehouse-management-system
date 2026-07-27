using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Identity.Application.Dtos;

namespace WMS.Modules.Identity.Application.Users;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthTokenDto>;
