using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class SolucionesMutation
    {
        // Inserta una nueva solución vinculada a una fricción
        public async Task<bool> InsertSolucion(
            SolucionesModel solucion,
            [Service] SolucionesServices solucionesServices,
            IResolverContext context)
        {
            return await solucionesServices.InsertSolucion(context, solucion);
        }

        // Actualiza los datos de una solución (estado, efectividad, nombre, etc.)
        public async Task<bool> UpdateSolucion(
            SolucionesModel solucion,
            [Service] SolucionesServices solucionesServices,
            IResolverContext context)
        {
            return await solucionesServices.UpdateSolucion(context, solucion);
        }

        // Elimina una solución por su identificador
        public async Task<bool> DeleteSolucion(
            Guid sol_id,
            [Service] SolucionesServices solucionesServices,
            IResolverContext context)
        {
            return await solucionesServices.DeleteSolucion(context, sol_id);
        }
    }
}