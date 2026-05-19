using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class PrioridadQuery
    {
        public async Task<IEnumerable<PrioridadModel>> GetPrioridades(
            [Service] PrioridadServices prioridadServices,
            IResolverContext context)
        {
            return await prioridadServices.GetPrioridades(context);
        }

        public async Task<IEnumerable<PrioridadModel>> GetPrioridadesByName(
            string nombre,
            [Service] PrioridadServices prioridadServices,
            IResolverContext context)
        {
            return await prioridadServices.GetPrioridadesByName(context, nombre);
        }

        public async Task<PrioridadModel?> GetPrioridadById(
            Guid pri_id,
            [Service] PrioridadServices prioridadServices,
            IResolverContext context)
        {
            return await prioridadServices.GetPrioridadById(context, pri_id);
        }
    }
}