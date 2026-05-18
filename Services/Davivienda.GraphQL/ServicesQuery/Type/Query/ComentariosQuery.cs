using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class ComentariosQuery
    {
        public async Task<IEnumerable<ComentariosModel>> GetComentarios(
            [Service] ComentariosServices comentariosServices,
            IResolverContext context)
        {
            return await comentariosServices.GetComentarios(context);
        }

        public async Task<IEnumerable<ComentariosModel>> GetComentariosByText(
            string texto,
            [Service] ComentariosServices comentariosServices,
            IResolverContext context)
        {
            return await comentariosServices.GetComentariosByText(context, texto);
        }

        public async Task<ComentariosModel?> GetComentarioById(
            Guid com_id,
            [Service] ComentariosServices comentariosServices,
            IResolverContext context)
        {
            return await comentariosServices.GetComentarioById(context, com_id);
        }
    }
}