using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class SolucionesMutation
    {
        public async Task<bool> InsertSolucion(
            SolucionesModel solucion,
            [Service] SolucionesServices solucionesServices,
            IResolverContext context)
        {
            return await solucionesServices.InsertSolucion(context, solucion);
        }

        public async Task<bool> UpdateSolucion(
            SolucionesModel solucion,
            [Service] SolucionesServices solucionesServices,
            IResolverContext context)
        {
            return await solucionesServices.UpdateSolucion(context, solucion);
        }

        public async Task<bool> DeleteSolucion(
            Guid sol_id,
            [Service] SolucionesServices solucionesServices,
            IResolverContext context)
        {
            return await solucionesServices.DeleteSolucion(context, sol_id);
        }
    }
}