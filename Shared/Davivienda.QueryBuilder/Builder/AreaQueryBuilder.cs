using Dapper.GraphQL;
using HotChocolate.Resolvers;
using Davivienda.Models;
using HotChocolate.Language;

namespace Davivienda.QueryBuilder.Builder
{
    public class AreaQueryBuilder : IQueryableBuilder<AreasModel>
    {
        public AreaQueryBuilder() { }

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

                    // Agregamos el campo y marcamos que al menos uno existe
                    query.Select($"{alias}.{fieldName.ToUpper()}");
                    addedFields = true;
                }
            }

            // ¡ESTA ES LA CLAVE! Si por alguna razón no detectó campos, 
            // forzamos un "*" para que la sintaxis de SQL no se rompa.
            if (!addedFields)
            {
                query.Select($"{alias}.*");
            }

            return query;
        }

        public SqlQueryContext BuildById(SqlQueryContext query, IResolverContext context, string alias)
        {
            // Primero cargamos los SELECT
            query = Build(query, context, alias);

            // Añadimos el filtro por ID usando el alias
            query.Where($"{alias}.ARE_ID = @area_id");
            return query;
        }

        // Cambia todas las menciones de "dbo.AREAS" por "dbo.AREA"
        public SqlInsertContext BuildInsert(SqlInsertContext query, IResolverContext context, string alias)
        {
            query.Insert("dbo.AREA");
            return query;
        }

        public SqlUpdateContext BuildUpdate(SqlUpdateContext query, IResolverContext context, string alias)
        {
            // Filtro para actualizaciones
            query.Where("ARE_ID = @ARE_ID");
            return query;
        }

        public SqlDeleteContext BuildDelete(SqlDeleteContext query, IResolverContext context, string alias)
        {
            // Aseguramos el nombre de la tabla con el esquema dbo para borrados
            query.Delete("dbo.AREA");
            return query;
        }
    }
}