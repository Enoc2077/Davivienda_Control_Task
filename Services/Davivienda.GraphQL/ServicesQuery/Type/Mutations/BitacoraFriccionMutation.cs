using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class BitacoraFriccionMutation
    {
        // Insertar nueva bitácora
        public async Task<bool> InsertBitacora(
            BitacoraFriccionModel bitacora,
            [Service] BitacoraFriccionServices bitacoraServices,
            IResolverContext context)
        {
            return await bitacoraServices.InsertBitacora(context, bitacora);
        }

        // Actualizar bitácora existente
        public async Task<bool> UpdateBitacora(
            BitacoraFriccionModel bitacora,
            [Service] BitacoraFriccionServices bitacoraServices,
            IResolverContext context)
        {
            return await bitacoraServices.UpdateBitacora(context, bitacora);
        }

        // Eliminar bitácora
        // Dentro de BitacoraSolucionesMutation.cs
        public async Task<bool> DeleteBitacoraSolucion(
            Guid bit_sol_id,
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            return await bitacoraService.DeleteBitacoraSolucion(context, bit_sol_id);
        }
    }
}
