using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class UsuarioQuery
    {
        // Obtiene la lista completa de usuarios
        public async Task<IEnumerable<UsuarioModel>> GetUsuarios(
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.GetUsuarios(context);
        }

        // --- MÉTODO AGREGADO PARA SOLUCIONAR EL ÚLTIMO ERROR SS0002 ---
        public async Task<UsuarioModel?> GetUsuarioByEmail(
            string email,
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.GetUsuarioByEmail(context, email);
        }

        // Obtiene los datos de un usuario específico por su ID (USU_ID)
        public async Task<UsuarioModel?> GetUsuarioById(
            Guid usu_id,
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.GetUsuarioById(context, usu_id);
        }
    }
}