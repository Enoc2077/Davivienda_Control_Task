using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class ComentariosMutation
    {
        // Insertar un nuevo comentario
        public async Task<bool> InsertComentario(
            ComentariosModel comentario,
            [Service] ComentariosServices comentariosServices,
            IResolverContext context)
        {
            return await comentariosServices.InsertComentario(context, comentario);
        }

        // Actualizar un comentario existente (Edición parcial soportada)
        public async Task<bool> UpdateComentario(
            ComentariosModel comentario,
            [Service] ComentariosServices comentariosServices,
            IResolverContext context)
        {
            return await comentariosServices.UpdateComentario(context, comentario);
        }

        // Eliminar un comentario de la base de datos
        public async Task<bool> DeleteComentario(
            Guid com_id,
            [Service] ComentariosServices comentariosServices,
            IResolverContext context)
        {
            return await comentariosServices.DeleteComentario(context, com_id);
        }
    }
}