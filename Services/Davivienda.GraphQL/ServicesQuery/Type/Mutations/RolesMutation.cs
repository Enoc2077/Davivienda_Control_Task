using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class RolesMutation
    {
        // Inserta un nuevo rol en el sistema
        public async Task<bool> InsertRol(
            RolesModel rol,
            [Service] RolesServices rolesServices,
            IResolverContext context)
        {
            return await rolesServices.InsertRol(context, rol);
        }

        // Actualiza un rol existente (Nombre, descripción o estado)
        public async Task<bool> UpdateRol(
            RolesModel rol,
            [Service] RolesServices rolesServices,
            IResolverContext context)
        {
            return await rolesServices.UpdateRol(context, rol);
        }

        // Elimina un rol por su ID
        public async Task<bool> DeleteRol(
            Guid rol_id,
            [Service] RolesServices rolesServices,
            IResolverContext context)
        {
            return await rolesServices.DeleteRol(context, rol_id);
        }
    }
}