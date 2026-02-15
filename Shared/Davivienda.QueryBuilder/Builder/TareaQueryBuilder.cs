using Dapper.GraphQL;
using HotChocolate.Resolvers;
using Davivienda.Models.Modelos;
using HotChocolate.Language;

namespace Davivienda.QueryBuilder.Builder
{
    public class TareaQueryBuilder : IQueryableBuilder<TareaModel>
    {
        public TareaQueryBuilder() { }

        public SqlQueryContext Build(SqlQueryContext query, IResolverContext context, string alias)
        {
            var selection = context.Selection.SyntaxNode.SelectionSet.Selections;
            bool addedFields = false;

            foreach (var item in selection)
            {
                if (item is FieldNode fieldNode)
                {
                    string fieldName = fieldNode.Name.Value;
                    if (fieldName == "__typename") continue;

                    query.Select($"{alias}.{fieldName.ToUpper()}");
                    addedFields = true;
                }
            }

            if (!addedFields) query.Select($"{alias}.*");

            return query;
        }

        public SqlQueryContext BuildById(SqlQueryContext query, IResolverContext context, string alias)
        {
            query = Build(query, context, alias);
            query.Where($"{alias}.TAR_ID = @tar_id");
            return query;
        }

        public SqlInsertContext BuildInsert(SqlInsertContext query, IResolverContext context, string alias)
        {
            query.Insert("dbo.TAREA");
            return query;
        }

        public SqlUpdateContext BuildUpdate(SqlUpdateContext query, IResolverContext context, string alias)
        {
            query.Where("TAR_ID = @TAR_ID");
            return query;
        }

        public SqlDeleteContext BuildDelete(SqlDeleteContext query, IResolverContext context, string alias)
        {
            query.Delete("dbo.TAREA");
            return query;
        }
    }
}