using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class ComentariosQuery
    {
        // Obtener todos los comentarios registrados
        public async Task<IEnumerable<ComentariosModel>> GetComentarios(
            [Service] ComentariosServices comentariosServices,
            IResolverContext context)
        {
            return await comentariosServices.GetComentarios(context);
        }

        // --- MÉTODO AGREGADO PARA SOLUCIONAR ERROR SS0002 ---
        public async Task<IEnumerable<ComentariosModel>> GetComentariosByText(
            string texto,
            [Service] ComentariosServices comentariosServices,
            IResolverContext context)
        {
            return await comentariosServices.GetComentariosByText(context, texto);
        }

        // Obtener un comentario específico filtrado por su ID (COM_ID)
        public async Task<ComentariosModel?> GetComentarioById(
            Guid com_id,
            [Service] ComentariosServices comentariosServices,
            IResolverContext context)
        {
            return await comentariosServices.GetComentarioById(context, com_id);
        }
    }
}