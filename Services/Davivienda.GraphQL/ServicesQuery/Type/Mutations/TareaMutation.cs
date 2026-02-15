using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class TareaMutation
    {
        // Inserta una nueva tarea vinculándola a un proceso, prioridad y usuario
        public async Task<bool> InsertTarea(
            TareaModel tarea,
            [Service] TareaServices tareaServices,
            IResolverContext context)
        {
            return await tareaServices.InsertTarea(context, tarea);
        }

        // Actualiza los datos de una tarea (cambio de estado, fechas o responsables)
        public async Task<bool> UpdateTarea(
            TareaModel tarea,
            [Service] TareaServices tareaServices,
            IResolverContext context)
        {
            return await tareaServices.UpdateTarea(context, tarea);
        }

        // Elimina una tarea por su identificador
        public async Task<bool> DeleteTarea(
            Guid tar_id,
            [Service] TareaServices tareaServices,
            IResolverContext context)
        {
            return await tareaServices.DeleteTarea(context, tar_id);
        }
    }
}