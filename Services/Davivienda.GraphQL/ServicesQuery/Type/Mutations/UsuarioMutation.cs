using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class UsuarioMutation
    {
        // Genera un token de acceso delegando toda la lógica al servicio
        // Esto soluciona los errores de tipos (string/int) que tenías aquí
        public async Task<string> Login(
            int usuNum,
            string password,
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            // El servicio ahora maneja las conversiones de UsuarioModel (string/Guid) a tipos simples
            return await usuarioServices.Login(context, usuNum, password);
        }

        // Registra un nuevo usuario en la plataforma
        public async Task<bool> InsertUsuario(
            UsuarioModel usuario,
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.InsertUsuario(context, usuario);
        }

        // Actualiza la información de un usuario existente
        public async Task<bool> UpdateUsuario(
            UsuarioModel usuario,
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.UpdateUsuario(context, usuario);
        }

        // Elimina permanentemente un usuario por su ID
        public async Task<bool> DeleteUsuario(
            Guid usu_id,
            [Service] UsuarioServices usuarioServices,
            IResolverContext context)
        {
            return await usuarioServices.DeleteUsuario(context, usu_id);
        }
    }
}