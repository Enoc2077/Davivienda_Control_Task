using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class BitacoraSolucionesQuery
    {
        // Obtener todo el listado de soluciones en bitácora
        public async Task<IEnumerable<BitacoraSolucionesModel>> GetBitacoraSoluciones(
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            return await bitacoraService.GetBitacoraSoluciones(context);
        }

        // Obtener una solución específica por su ID único
        public async Task<BitacoraSolucionesModel?> GetBitacoraSolucionById(
            Guid bit_sol_id,
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            return await bitacoraService.GetBitacoraSolucionById(context, bit_sol_id);
        }
        // Obtener bitácoras de soluciones filtradas por nombre
        public async Task<IEnumerable<BitacoraSolucionesModel>> GetBitacoraSolucionesByName(
            string nombre,
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            // Nota: Asegúrate de que este método exista en tu clase BitacoraSolucionesServices
            return await bitacoraService.GetBitacoraSolucionesByName(context, nombre);
        }
    }
}