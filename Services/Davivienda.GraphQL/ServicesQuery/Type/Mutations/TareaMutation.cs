using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class TareaMutation
    {
        public async Task<bool> InsertTarea(
            TareaModel tarea,
            [Service] TareaServices tareaServices,
            IResolverContext context)
        {
            return await tareaServices.InsertTarea(context, tarea);
        }

        public async Task<bool> UpdateTarea(
            TareaModel tarea,
            [Service] TareaServices tareaServices,
            IResolverContext context)
        {
            return await tareaServices.UpdateTarea(context, tarea);
        }

        public async Task<bool> DeleteTarea(
            Guid tar_id,
            [Service] TareaServices tareaServices,
            IResolverContext context)
        {
            return await tareaServices.DeleteTarea(context, tar_id);
        }
    }
}