using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class ProcesoQuery
    {
        // Obtiene el listado de todos los procesos registrados
        public async Task<IEnumerable<ProcesoModel>> GetProcesos(
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.GetProcesos(context);
        }

        // --- MÉTODO AGREGADO PARA SOLUCIONAR ERROR SS0002 ---
        public async Task<IEnumerable<ProcesoModel>> GetProcesosByName(
            string nombre,
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.GetProcesosByName(context, nombre);
        }

        // Obtiene un proceso específico por su identificador único (PROC_ID)
        public async Task<ProcesoModel?> GetProcesoById(
            Guid proc_id,
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.GetProcesoById(context, proc_id);
        }
    }
}