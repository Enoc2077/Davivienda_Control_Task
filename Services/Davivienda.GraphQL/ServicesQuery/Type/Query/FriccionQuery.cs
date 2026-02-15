using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class FriccionQuery
    {
        // Obtiene el listado completo de fricciones
        public async Task<IEnumerable<FriccionModel>> GetFricciones(
            [Service] FriccionServices friccionServices,
            IResolverContext context)
        {
            return await friccionServices.GetFricciones(context);
        }

        // Obtiene una fricción específica por su identificador único (FRI_ID)
        public async Task<FriccionModel?> GetFriccionById(
            Guid fri_id,
            [Service] FriccionServices friccionServices,
            IResolverContext context)
        {
            return await friccionServices.GetFriccionById(context, fri_id);
        }
    }
}