using Dapper.GraphQL;
using HotChocolate.Resolvers;
using Davivienda.Models.Modelos;
using HotChocolate.Language;

namespace Davivienda.QueryBuilder.Builder
{
    public class BitacoraFriccionQueryBuilder : IQueryableBuilder<BitacoraFriccionModel>
    {
        public BitacoraFriccionQueryBuilder() { }

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

                    // Esto funciona si tu modelo tiene BIT_FRI_ID, BIT_FRI_NOM, etc.
                    query.Select($"{alias}.{fieldName.ToUpper()}");
                    addedFields = true;
                }
            }

            if (!addedFields)
            {
                query.Select($"{alias}.*");
            }

            return query;
        }

        public SqlQueryContext BuildById(SqlQueryContext query, IResolverContext context, string alias)
        {
            query = Build(query, context, alias);
            // CORRECCIÓN: Nombre de la columna PK real
            query.Where($"{alias}.BIT_FRI_ID = @bit_fri_id");
            return query;
        }

        public SqlInsertContext BuildInsert(SqlInsertContext query, IResolverContext context, string alias)
        {
            // CORRECCIÓN: Nombre de la tabla en plural
            query.Insert("dbo.BITACORA_FRICCIONES");
            return query;
        }

        public SqlUpdateContext BuildUpdate(SqlUpdateContext query, IResolverContext context, string alias)
        {
            // CORRECCIÓN: Nombre de la columna PK real
            query.Where("BIT_FRI_ID = @BIT_FRI_ID");
            return query;
        }

        public SqlDeleteContext BuildDelete(SqlDeleteContext query, IResolverContext context, string alias)
        {
            // CORRECCIÓN: Nombre de la tabla en plural
            query.Delete("dbo.BITACORA_FRICCIONES");
            return query;
        }
    }
}