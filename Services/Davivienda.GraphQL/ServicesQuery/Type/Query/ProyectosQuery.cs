using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class ProyectosQuery
    {
        public async Task<IEnumerable<ProyectosModel>> GetProyectos(
            [Service] ProyectosServices proyectosServices,
            IResolverContext context)
        {
            return await proyectosServices.GetProyectos(context);
        }

        public async Task<IEnumerable<ProyectosModel>> GetProyectosByName(
            string nombre,
            [Service] ProyectosServices proyectosServices,
            IResolverContext context)
        {
            return await proyectosServices.GetProyectosByName(context, nombre);
        }

        public async Task<ProyectosModel?> GetProyectoById(
            Guid pro_id,
            [Service] ProyectosServices proyectosServices,
            IResolverContext context)
        {
            return await proyectosServices.GetProyectoById(context, pro_id);
        }
    }
}