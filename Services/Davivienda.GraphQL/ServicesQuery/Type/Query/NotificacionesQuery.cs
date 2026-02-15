using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class NotificacionesQuery
    {
        // Obtener el listado de todas las notificaciones
        public async Task<IEnumerable<NotificacionesModel>> GetNotificaciones(
            [Service] NotificacionesServices notiServices,
            IResolverContext context)
        {
            return await notiServices.GetNotificaciones(context);
        }

        // Obtener una notificación específica por su ID (NOT_ID)
        public async Task<NotificacionesModel?> GetNotificacionById(
            Guid not_id,
            [Service] NotificacionesServices notiServices,
            IResolverContext context)
        {
            return await notiServices.GetNotificacionById(context, not_id);
        }
    }
}