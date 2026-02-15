using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class BitacoraSolucionesMutation
    {
        // Crear un nuevo registro de solución
        public async Task<bool> InsertBitacoraSolucion(
            BitacoraSolucionesModel bitacora,
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            return await bitacoraService.InsertBitacoraSolucion(context, bitacora);
        }

        // Actualizar datos de una solución existente
        public async Task<bool> UpdateBitacoraSolucion(
            BitacoraSolucionesModel bitacora,
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            return await bitacoraService.UpdateBitacoraSolucion(context, bitacora);
        }

        // Eliminar registro de bitácora
        public async Task<bool> DeleteBitacoraSolucion(
            Guid bit_sol_id,
            [Service] BitacoraSolucionesServices bitacoraService,
            IResolverContext context)
        {
            // Nota: Asegúrate de tener el método Delete en tu Service si habilitas esta línea
            // return await bitacoraService.DeleteBitacoraSolucion(context, bit_sol_id);
            return false;
        }
    }
}