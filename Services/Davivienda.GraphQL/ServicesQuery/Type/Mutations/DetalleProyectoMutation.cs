using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class DetalleProyectoMutation
    {
        public async Task<bool> InsertDetalleProyecto(
            DetalleProyectoModel detalle,
            [Service] DetalleProyectoServices detalleServices,
            IResolverContext context)
        {
            return await detalleServices.InsertDetalleProyecto(context, detalle);
        }

        public async Task<bool> UpdateDetalleProyecto(
            DetalleProyectoModel detalle,
            [Service] DetalleProyectoServices detalleServices,
            IResolverContext context)
        {
            return await detalleServices.UpdateDetalleProyecto(context, detalle);
        }

        public async Task<bool> DeleteDetalleProyecto(
            Guid det_pro_id,
            [Service] DetalleProyectoServices detalleServices,
            IResolverContext context)
        {
            return await detalleServices.DeleteDetalleProyecto(context, det_pro_id);
        }
    }
}