using Dapper.GraphQL;
using HotChocolate.Resolvers;

namespace Davivienda.QueryBuilder
{
    public interface IQueryableBuilder<T>
    {
        SqlQueryContext Build(SqlQueryContext query, IResolverContext context, string alias);
        SqlQueryContext BuildById(SqlQueryContext query, IResolverContext context, string alias);
        SqlInsertContext BuildInsert(SqlInsertContext query, IResolverContext context, string alias);
        SqlUpdateContext BuildUpdate(SqlUpdateContext query, IResolverContext context, string alias);
        SqlDeleteContext BuildDelete(SqlDeleteContext query, IResolverContext context, string alias);
    }
}