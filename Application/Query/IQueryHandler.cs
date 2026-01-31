namespace Application.Query;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery
    where TResponse : IQueryResponse
{
    Task<TResponse> HandleAsync(TQuery query);
}
