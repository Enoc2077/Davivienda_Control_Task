using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class FriccionMutation
    {
        public async Task<bool> InsertFriccion(
            FriccionModel friccion,
            [Service] FriccionServices friccionServices,
            IResolverContext context)
        {
            return await friccionServices.InsertFriccion(context, friccion);
        }

        public async Task<bool> UpdateFriccion(
            FriccionModel friccion,
            [Service] FriccionServices friccionServices,
            IResolverContext context)
        {
            return await friccionServices.UpdateFriccion(context, friccion);
        }

        public async Task<bool> DeleteFriccion(
            Guid fri_id,
            [Service] FriccionServices friccionServices,
            IResolverContext context)
        {
            return await friccionServices.DeleteFriccion(context, fri_id);
        }
    }
}