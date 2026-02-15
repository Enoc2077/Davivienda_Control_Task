using Davivienda.Models;
using HotChocolate.Resolvers;
using HotChocolate.Subscriptions;
using Davivienda.GraphQL.ServicesQuery.Services;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutations
{
    [ExtendObjectType("Mutation")]
    public class AreaMutation
    {
        public  async Task<bool> InsertAreas(
           IResolverContext context,
           [Service] AreaServices areasServices,
           AreasModel area,
           [Service] ITopicEventSender eventSender,
           CancellationToken cancellationToken)
        {
            // Notifica el evento de inserción
            await eventSender.SendAsync(nameof(InsertAreas), area);
            return await areasServices.InsertArea(context, area);
        }

        // Eliminar Área
        public  async Task<bool> DeleteArea(
            IResolverContext context,
            [Service] AreaServices areasServices,
            Guid area_id)
        {
            // Nota: En tu oficina el método en el service se llama DeleteAreas (en plural)
            return await areasServices.DeleteAreas(context, area_id);
        }

        // Actualizar Área
        public  async Task<bool> UpdateArea(
            IResolverContext context,
            [Service] AreaServices areasServices,
            AreasModel areas,
            [Service] ITopicEventSender topicEventSender)
        {
            // Notifica usando el nombre de la suscripción
            await topicEventSender.SendAsync("UpdateArea", areas);
            return await areasServices.UpdateAreas(context, areas);
        }

    }
}
