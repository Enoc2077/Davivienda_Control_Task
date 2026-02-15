using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class RolesQuery
    {
        // Obtiene la lista completa de roles definidos
        public async Task<IEnumerable<RolesModel>> GetRoles(
            [Service] RolesServices rolesServices,
            IResolverContext context)
        {
            return await rolesServices.GetRoles(context);
        }

        // --- MÉTODO AGREGADO PARA SOLUCIONAR EL ERROR SS0002 ---
        public async Task<IEnumerable<RolesModel>> GetRolesByName(
            string nombre,
            [Service] RolesServices rolesServices,
            IResolverContext context)
        {
            return await rolesServices.GetRolesByName(context, nombre);
        }

        // Obtiene un rol específico por su identificador único (ROL_ID)
        public async Task<RolesModel?> GetRolById(
            Guid rol_id,
            [Service] RolesServices rolesServices,
            IResolverContext context)
        {
            return await rolesServices.GetRolById(context, rol_id);
        }
    }
}