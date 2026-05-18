using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class TareaQuery
    {
        public async Task<IEnumerable<TareaModel>> GetTareas(
            [Service] TareaServices tareaServices,
            IResolverContext context)
        {
            return await tareaServices.GetTareas(context);
        }

        public async Task<IEnumerable<TareaModel>> GetTareasByName(
            string nombre,
            [Service] TareaServices tareaServices,
            IResolverContext context)
        {
            return await tareaServices.GetTareasByName(context, nombre);
        }

        public async Task<TareaModel?> GetTareaById(
            Guid tar_id,
            [Service] TareaServices tareaServices,
            IResolverContext context)
        {
            return await tareaServices.GetTareaById(context, tar_id);
        }
    }
}