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
            await eventSender.SendAsync(nameof(InsertAreas), area);
            return await areasServices.InsertArea(context, area);
        }

        // Eliminar Área
        public  async Task<bool> DeleteArea(
            IResolverContext context,
            [Service] AreaServices areasServices,
            Guid area_id)
        {
            return await areasServices.DeleteAreas(context, area_id);
        }

        // Actualizar Área
        public  async Task<bool> UpdateArea(
            IResolverContext context,
            [Service] AreaServices areasServices,
            AreasModel areas,
            [Service] ITopicEventSender topicEventSender)
        {
            await topicEventSender.SendAsync("UpdateArea", areas);
            return await areasServices.UpdateAreas(context, areas);
        }

    }
}
