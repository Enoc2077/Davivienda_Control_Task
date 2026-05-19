using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class UsuarioMutation
    {
        // --- AUTENTICACIÓN ---

        public async Task<LoginModel.LoginResponse> Login(
            [Service] UsuarioServices usuarioServices,
            LoginModel.LoginInput input)
        {
            return await usuarioServices.LoginAsync(input);
        }

        // --- USUARIO ---

        public async Task<bool> InsertUsuario(
            UsuarioModel usuario,
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.InsertUsuario(context, usuario);
        }

        public async Task<bool> UpdateUsuario(
            UsuarioModel usuario,
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.UpdateUsuario(context, usuario);
        }

        public async Task<bool> DeleteUsuario(
            Guid usu_id,
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.DeleteUsuario(context, usu_id);
        }
    }
}