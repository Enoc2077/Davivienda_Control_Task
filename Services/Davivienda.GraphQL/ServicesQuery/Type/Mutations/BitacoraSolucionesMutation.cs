using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class BitacoraSolucionesMutation
    {
        public async Task<bool> InsertBitacoraSolucion(
            BitacoraSolucionesModel bitacora,
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            return await bitacoraService.InsertBitacoraSolucion(context, bitacora);
        }

        public async Task<bool> UpdateBitacoraSolucion(
            BitacoraSolucionesModel bitacora,
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            return await bitacoraService.UpdateBitacoraSolucion(context, bitacora);
        }

        public async Task<bool> DeleteBitacoraSolucion(
            Guid bit_sol_id,
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            return false;
        }
    }
}