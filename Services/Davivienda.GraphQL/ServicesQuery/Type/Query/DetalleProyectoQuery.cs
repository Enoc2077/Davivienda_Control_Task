using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class DetalleProyectoQuery
    {
        public async Task<IEnumerable<DetalleProyectoModel>> GetDetallesProyecto(
            [Service] DetalleProyectoServices detalleServices,
            IResolverContext context)
        {
            return await detalleServices.GetDetallesProyecto(context);
        }

        public async Task<DetalleProyectoModel?> GetDetalleProyectoById(
            Guid det_pro_id,
            [Service] DetalleProyectoServices detalleServices,
            IResolverContext context)
        {
            return await detalleServices.GetDetalleProyectoById(context, det_pro_id);
        }
    }
}