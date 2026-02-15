using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class ProyectosQuery
    {
        // Obtiene el listado de todos los proyectos registrados
        public async Task<IEnumerable<ProyectosModel>> GetProyectos(
            [Service] ProyectosServices proyectosServices,
            IResolverContext context)
        {
            return await proyectosServices.GetProyectos(context);
        }

        // --- MÉTODO AGREGADO PARA SOLUCIONAR ERROR SS0002 ---
        public async Task<IEnumerable<ProyectosModel>> GetProyectosByName(
            string nombre,
            [Service] ProyectosServices proyectosServices,
            IResolverContext context)
        {
            return await proyectosServices.GetProyectosByName(context, nombre);
        }

        // Obtiene un proyecto específico filtrado por su ID único (PRO_ID)
        public async Task<ProyectosModel?> GetProyectoById(
            Guid pro_id,
            [Service] ProyectosServices proyectosServices,
            IResolverContext context)
        {
            return await proyectosServices.GetProyectoById(context, pro_id);
        }
    }
}