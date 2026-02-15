using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class DetalleProyectoMutation
    {
        // Crea una nueva relación entre Proyecto, Usuario y Rol
        public async Task<bool> InsertDetalleProyecto(
            DetalleProyectoModel detalle,
            [Service] DetalleProyectoServices detalleServices,
            IResolverContext context)
        {
            return await detalleServices.InsertDetalleProyecto(context, detalle);
        }

        // Actualiza una asignación existente (ej. cambiar el rol o el usuario asignado)
        public async Task<bool> UpdateDetalleProyecto(
            DetalleProyectoModel detalle,
            [Service] DetalleProyectoServices detalleServices,
            IResolverContext context)
        {
            return await detalleServices.UpdateDetalleProyecto(context, detalle);
        }

        // Elimina una asignación de la base de datos
        public async Task<bool> DeleteDetalleProyecto(
            Guid det_pro_id,
            [Service] DetalleProyectoServices detalleServices,
            IResolverContext context)
        {
            return await detalleServices.DeleteDetalleProyecto(context, det_pro_id);
        }
    }
}