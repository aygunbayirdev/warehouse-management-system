using MediatR;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Application.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
