using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class ProyectosMutation
    {
        public async Task<bool> InsertProyecto(
            ProyectosModel proyecto,
            [Service] ProyectosServices proyectosServices,
            IResolverContext context)
        {
            return await proyectosServices.InsertProyecto(context, proyecto);
        }

        public async Task<bool> UpdateProyecto(
            ProyectosModel proyecto,
            [Service] ProyectosServices proyectosServices,
            IResolverContext context)
        {
            return await proyectosServices.UpdateProyecto(context, proyecto);
        }

        public async Task<bool> DeleteProyecto(
            Guid pro_id,
            [Service] ProyectosServices proyectosServices,
            IResolverContext context)
        {
            return await proyectosServices.DeleteProyecto(context, pro_id);
        }
    }
}