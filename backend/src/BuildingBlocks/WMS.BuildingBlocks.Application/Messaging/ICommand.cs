using MediatR;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Application.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
