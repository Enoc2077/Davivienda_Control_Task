using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class BitacoraSolucionesQuery
    {
        public async Task<IEnumerable<BitacoraSolucionesModel>> GetBitacoraSoluciones(
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            return await bitacoraService.GetBitacoraSoluciones(context);
        }

        public async Task<BitacoraSolucionesModel?> GetBitacoraSolucionById(
            Guid bit_sol_id,
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            return await bitacoraService.GetBitacoraSolucionById(context, bit_sol_id);
        }
        public async Task<IEnumerable<BitacoraSolucionesModel>> GetBitacoraSolucionesByName(
            string nombre,
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            return await bitacoraService.GetBitacoraSolucionesByName(context, nombre);
        }
    }
}