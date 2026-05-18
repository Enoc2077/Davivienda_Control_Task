using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class NotificacionesQuery
    {
        public async Task<IEnumerable<NotificacionesModel>> GetNotificaciones(
            [Service] NotificacionesServices notiServices,
            IResolverContext context)
        {
            return await notiServices.GetNotificaciones(context);
        }

        public async Task<NotificacionesModel?> GetNotificacionById(
            Guid not_id,
            [Service] NotificacionesServices notiServices,
            IResolverContext context)
        {
            return await notiServices.GetNotificacionById(context, not_id);
        }
    }
}