using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class ProcesoMutation
    {
        public async Task<bool> InsertProceso(
            ProcesoModel proceso,
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.InsertProceso(context, proceso);
        }

        public async Task<bool> UpdateProceso(
            ProcesoModel proceso,
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.UpdateProceso(context, proceso);
        }

        public async Task<bool> DeleteProceso(
            Guid proc_id,
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.DeleteProceso(context, proc_id);
        }
    }
}