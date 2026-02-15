using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder; // Asegúrate de que esta referencia exista
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class RolesServices
    {
        private readonly DataBase dataBase;
        private readonly RolesQueryBuilder rolBuilder; // Inyección del Builder

        public RolesServices(DataBase dataBase, RolesQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.rolBuilder = builder; // Asignación del builder
        }

        // --- QUERIES ---

        public async Task<IEnumerable<RolesModel>> GetRoles(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"r.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "r.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.ROLES r";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<RolesModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<RolesModel>> GetRolesByName(IResolverContext context, string nombre)
        {
            try
            {
                // Búsqueda por coincidencia en ROL_NOM
                string sqlQuery = "SELECT r.* FROM dbo.ROLES r WHERE r.ROL_NOM LIKE @nombre";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<RolesModel>(
                    sqlQuery,
                    new { nombre = $"%{nombre}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetRolesByName: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<RolesModel?> GetRolById(IResolverContext context, Guid rol_id)
        {
            try
            {
                string sqlQuery = "SELECT r.* FROM dbo.ROLES r WHERE r.ROL_ID = @rol_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<RolesModel>(sqlQuery, new { rol_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        // --- MUTATIONS ---

        public async Task<bool> InsertRol(IResolverContext context, RolesModel rol)
        {
            try
            {
                if (rol.ROL_ID == Guid.Empty) rol.ROL_ID = Guid.NewGuid();

                // Mantenemos tu lógica de omitir fechas si así está en tu DB
                string sqlQuery = @"INSERT INTO dbo.ROLES 
                                    (ROL_ID, ROL_NOM, ROL_DES, ROL_EST) 
                                    VALUES 
                                    (@ROL_ID, @ROL_NOM, @ROL_DES, @ROL_EST)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, rol);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateRol(IResolverContext context, RolesModel rol)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<RolesModel>(
                    "SELECT * FROM dbo.ROLES WHERE ROL_ID = @ROL_ID", new { rol.ROL_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.ROLES SET 
                                    ROL_NOM = @ROL_NOM, 
                                    ROL_DES = @ROL_DES, 
                                    ROL_EST = @ROL_EST 
                                    WHERE ROL_ID = @ROL_ID";

                var parameters = new
                {
                    ROL_ID = rol.ROL_ID,
                    ROL_NOM = !string.IsNullOrEmpty(rol.ROL_NOM) ? rol.ROL_NOM : existing.ROL_NOM,
                    ROL_DES = rol.ROL_DES ?? existing.ROL_DES,
                    ROL_EST = rol.ROL_EST
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteRol(IResolverContext context, Guid rol_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.ROLES WHERE ROL_ID = @rol_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { rol_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}