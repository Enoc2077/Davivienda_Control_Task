using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class AreaQuery
    {
        public  async Task<IEnumerable<AreasModel>> GetAreas(
            IResolverContext context,
            [Service] AreaServices areasServices)
        {
            var data = await areasServices.GetAreas(context);
            return data;
        }

        // Obtener áreas por nombre
        public  async Task<IEnumerable<AreasModel>> GetAreasByName(
            string area_name,
            IResolverContext context,
            [Service] AreaServices areasServices)
        {
            var data = await areasServices.GetAreasByName(context, area_name);
            return data;
        }

        // Obtener área por ID
        public  async Task<AreasModel> GetAreasById(
            Guid area_id,
            IResolverContext context,
            [Service] AreaServices areasServices)
        {
            var data = await areasServices.GetAreaById(context, area_id);
            return data;
        }

    }
}
