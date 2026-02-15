using Dapper.GraphQL;
using HotChocolate.Resolvers;
using Davivienda.Models.Modelos;
using HotChocolate.Language;

namespace Davivienda.QueryBuilder.Builder
{
    public class ProyectosQueryBuilder : IQueryableBuilder<ProyectosModel>
    {
        public ProyectosQueryBuilder() { }

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
            query.Where($"{alias}.PRO_ID = @pro_id");
            return query;
        }

        public SqlInsertContext BuildInsert(SqlInsertContext query, IResolverContext context, string alias)
        {
            query.Insert("dbo.PROYECTOS");
            return query;
        }

        public SqlUpdateContext BuildUpdate(SqlUpdateContext query, IResolverContext context, string alias)
        {
            query.Where("PRO_ID = @PRO_ID");
            return query;
        }

        public SqlDeleteContext BuildDelete(SqlDeleteContext query, IResolverContext context, string alias)
        {
            query.Delete("dbo.PROYECTOS");
            return query;
        }
    }
}