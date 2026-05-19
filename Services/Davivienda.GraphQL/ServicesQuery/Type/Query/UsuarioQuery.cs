using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class UsuarioQuery
    {
        public async Task<IEnumerable<UsuarioModel>> GetUsuarios(
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.GetUsuarios(context);
        }

        public async Task<UsuarioModel?> GetUsuarioByEmail(
            string email,
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.GetUsuarioByEmail(context, email);
        }

        public async Task<UsuarioModel?> GetUsuarioById(
            Guid usu_id,
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.GetUsuarioById(context, usu_id);
        }
    }
}