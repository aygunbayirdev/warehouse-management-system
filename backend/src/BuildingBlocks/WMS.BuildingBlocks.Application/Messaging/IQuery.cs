using MediatR;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
