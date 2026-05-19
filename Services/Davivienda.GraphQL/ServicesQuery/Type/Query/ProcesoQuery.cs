using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class ProcesoQuery
    {
        public async Task<IEnumerable<ProcesoModel>> GetProcesos(
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.GetProcesos(context);
        }

        public async Task<IEnumerable<ProcesoModel>> GetProcesosByName(
            string nombre,
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.GetProcesosByName(context, nombre);
        }

        public async Task<ProcesoModel?> GetProcesoById(
            Guid proc_id,
            [Service] ProcesoServices procesoServices,
            IResolverContext context)
        {
            return await procesoServices.GetProcesoById(context, proc_id);
        }
    }
}