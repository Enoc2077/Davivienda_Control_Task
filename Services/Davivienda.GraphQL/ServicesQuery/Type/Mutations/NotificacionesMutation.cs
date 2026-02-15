using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class NotificacionesMutation
    {
        // Crear una nueva notificación para un usuario
        public async Task<bool> InsertNotificacion(
            NotificacionesModel notificacion,
            [Service] NotificacionesServices notiServices,
            IResolverContext context)
        {
            return await notiServices.InsertNotificacion(context, notificacion);
        }

        // Actualizar una notificación (Útil para marcar como NOT_LEI = true)
        public async Task<bool> UpdateNotificacion(
            NotificacionesModel notificacion,
            [Service] NotificacionesServices notiServices,
            IResolverContext context)
        {
            return await notiServices.UpdateNotificacion(context, notificacion);
        }

        // Eliminar una notificación de la base de datos
        public async Task<bool> DeleteNotificacion(
            Guid not_id,
            [Service] NotificacionesServices notiServices,
            IResolverContext context)
        {
            return await notiServices.DeleteNotificacion(context, not_id);
        }
    }
}