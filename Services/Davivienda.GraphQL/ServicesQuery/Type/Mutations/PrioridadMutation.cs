using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class PrioridadMutation
    {
        // Insertar un nuevo nivel de prioridad
        public async Task<bool> InsertPrioridad(
            PrioridadModel prioridad,
            [Service] PrioridadServices prioridadServices,
            IResolverContext context)
        {
            return await prioridadServices.InsertPrioridad(context, prioridad);
        }

        // Actualizar datos de una prioridad (Nombre, Descripción o Nivel)
        public async Task<bool> UpdatePrioridad(
            PrioridadModel prioridad,
            [Service] PrioridadServices prioridadServices,
            IResolverContext context)
        {
            return await prioridadServices.UpdatePrioridad(context, prioridad);
        }

        // Eliminar un registro de prioridad
        public async Task<bool> DeletePrioridad(
            Guid pri_id,
            [Service] PrioridadServices prioridadServices,
            IResolverContext context)
        {
            return await prioridadServices.DeletePrioridad(context, pri_id);
        }
    }
}