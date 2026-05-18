using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class NotificacionesMutation
    {
        public async Task<bool> InsertNotificacion(
            NotificacionesModel notificacion,
            [Service] NotificacionesServices notiServices,
            IResolverContext context)
        {
            return await notiServices.InsertNotificacion(context, notificacion);
        }

        public async Task<bool> UpdateNotificacion(
            NotificacionesModel notificacion,
            [Service] NotificacionesServices notiServices,
            IResolverContext context)
        {
            return await notiServices.UpdateNotificacion(context, notificacion);
        }

        public async Task<bool> DeleteNotificacion(
            Guid not_id,
            [Service] NotificacionesServices notiServices,
            IResolverContext context)
        {
            return await notiServices.DeleteNotificacion(context, not_id);
        }
    }
}