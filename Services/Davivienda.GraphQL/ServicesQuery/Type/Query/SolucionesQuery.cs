using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class SolucionesQuery
    {
        // Obtiene el listado de todas las soluciones
        public async Task<IEnumerable<SolucionesModel>> GetSoluciones(
            [Service] SolucionesServices solucionesServices,
            IResolverContext context)
        {
            return await solucionesServices.GetSoluciones(context);
        }

        // --- MÉTODO AGREGADO PARA SOLUCIONAR ERROR SS0002 ---
        public async Task<IEnumerable<SolucionesModel>> GetSolucionesByName(
            string nombre,
            [Service] SolucionesServices solucionesServices,
            IResolverContext context)
        {
            return await solucionesServices.GetSolucionesByName(context, nombre);
        }

        // Obtiene una solución específica por su ID único (SOL_ID)
        public async Task<SolucionesModel?> GetSolucionById(
            Guid sol_id,
            [Service] SolucionesServices solucionesServices,
            IResolverContext context)
        {
            return await solucionesServices.GetSolucionById(context, sol_id);
        }
    }
}