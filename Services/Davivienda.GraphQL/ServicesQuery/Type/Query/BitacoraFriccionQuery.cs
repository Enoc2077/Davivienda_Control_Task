using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class BitacoraFriccionQuery
    {
        // Obtener todos los registros
        public async Task<IEnumerable<BitacoraFriccionModel>> GetBitacoras(
            [Service] BitacoraFriccionServices bitacoraServices,
            IResolverContext context)
        {
            return await bitacoraServices.GetBitacoras(context);
        }

        // Obtener un registro por ID
        public async Task<BitacoraFriccionModel?> GetBitacoraById(
            Guid bif_id,
            [Service] BitacoraFriccionServices bitacoraServices,
            IResolverContext context)
        {
            return await bitacoraServices.GetBitacoraById(context, bif_id);
        }
    }
}