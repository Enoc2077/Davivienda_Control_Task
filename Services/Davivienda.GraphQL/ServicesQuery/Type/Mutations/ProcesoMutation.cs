using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class ProcesoMutation
    {
        // Inserta un nuevo proceso dentro de un proyecto
        public async Task<bool> InsertProceso(
            ProcesoModel proceso,
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.InsertProceso(context, proceso);
        }

        // Actualiza la información de un proceso (nombre, estado, frecuencia, etc.)
        public async Task<bool> UpdateProceso(
            ProcesoModel proceso,
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.UpdateProceso(context, proceso);
        }

        // Elimina un proceso de la base de datos
        public async Task<bool> DeleteProceso(
            Guid proc_id,
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.DeleteProceso(context, proc_id);
        }
    }
}