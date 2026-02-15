using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using Davivienda.GraphQL.Security; // Asegúrate de tener este using
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class UsuarioServices
    {
        private readonly DataBase dataBase;
        private readonly UsuarioQueryBuilder usuBuilder;
        private readonly JwtProvider jwtProvider; // Añadido para manejar el token

        public UsuarioServices(DataBase dataBase, UsuarioQueryBuilder builder, JwtProvider jwtProvider)
        {
            this.dataBase = dataBase;
            this.usuBuilder = builder;
            this.jwtProvider = jwtProvider;
        }

        // --- NUEVO MÉTODO DE LOGIN (CON LÓGICA INTEGRADA) ---
        public async Task<string> Login(IResolverContext context, int usuNum, string password)
        {
            try
            {
                // 1. Buscamos al usuario por su número de empleado
                var usuario = await GetUsuarioByEmpleadoNum(usuNum);

                // 2. Validaciones de credenciales
                if (usuario == null || usuario.USU_CON != password)
                {
                    throw new GraphQLException("Número de empleado o contraseña incorrectos.");
                }

                // 3. Validación de estado
                if (usuario.USU_EST == false)
                {
                    throw new GraphQLException("El usuario se encuentra inactivo.");
                }

                // 4. Generación del Token (Aquí manejamos los nulos como pediste)
                return jwtProvider.GenerarToken(
                        int.TryParse(usuario.USU_NUM, out int num) ? num : 0, // Convierte string a int
                        usuario.USU_NOM ?? "Usuario",                        // Maneja el nombre
                        0                                                    // ROL_ID es Guid, aquí debes decidir si 
                                                                             // envías el GUID como string o un ID entero
                    );
            }
            catch (Exception ex)
            {
                throw new GraphQLException(ex.Message);
            }
        }




        // --- CONSULTAS ---

        public async Task<IEnumerable<UsuarioModel>> GetUsuarios(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"u.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "u.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.USUARIO u";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<UsuarioModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<UsuarioModel?> GetUsuarioByEmail(IResolverContext context, string email)
        {
            try
            {
                string sqlQuery = "SELECT u.* FROM dbo.USUARIO u WHERE u.USU_COR = @email";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<UsuarioModel>(sqlQuery, new { email });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetUsuarioByEmail: {ex.Message}", ex);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<UsuarioModel?> GetUsuarioById(IResolverContext context, Guid usu_id)
        {
            try
            {
                string sqlQuery = "SELECT u.* FROM dbo.USUARIO u WHERE u.USU_ID = @usu_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<UsuarioModel>(sqlQuery, new { usu_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<UsuarioModel?> GetUsuarioByEmpleadoNum(int usu_num)
        {
            try
            {
                string sqlQuery = "SELECT u.* FROM dbo.USUARIO u WHERE u.USU_NUM = @usu_num";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<UsuarioModel>(sqlQuery, new { usu_num });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetUsuarioByEmpleadoNum: {ex.Message}", ex);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        // --- MUTACIONES ---

        public async Task<bool> InsertUsuario(IResolverContext context, UsuarioModel usuario)
        {
            try
            {
                if (usuario.USU_ID == Guid.Empty) usuario.USU_ID = Guid.NewGuid();
                if (usuario.USU_FEC_CRE == default) usuario.USU_FEC_CRE = DateTimeOffset.Now;
                usuario.USU_EST ??= true;

                string sqlQuery = @"INSERT INTO dbo.USUARIO 
                            (USU_ID, USU_NOM, USU_NUM, USU_COR, USU_CON, USU_TEL, USU_EST, ROL_ID, ARE_ID, USU_FEC_CRE, USU_FEC_MOD) 
                            VALUES 
                            (@USU_ID, @USU_NOM, @USU_NUM, @USU_COR, @USU_CON, @USU_TEL, @USU_EST, @ROL_ID, @ARE_ID, @USU_FEC_CRE, @USU_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, usuario);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateUsuario(IResolverContext context, UsuarioModel usuario)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<UsuarioModel>(
                    "SELECT * FROM dbo.USUARIO WHERE USU_ID = @USU_ID", new { usuario.USU_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.USUARIO SET 
                            USU_NOM = @USU_NOM, USU_NUM = @USU_NUM, USU_COR = @USU_COR, 
                            USU_CON = @USU_CON, USU_TEL = @USU_TEL, USU_EST = @USU_EST, 
                            ROL_ID = @ROL_ID, ARE_ID = @ARE_ID, USU_FEC_MOD = @USU_FEC_MOD 
                            WHERE USU_ID = @USU_ID";

                var parameters = new
                {
                    USU_ID = usuario.USU_ID,
                    USU_NOM = !string.IsNullOrEmpty(usuario.USU_NOM) ? usuario.USU_NOM : existing.USU_NOM,
                    USU_NUM = usuario.USU_NUM ?? existing.USU_NUM,
                    USU_COR = usuario.USU_COR ?? existing.USU_COR,
                    USU_CON = usuario.USU_CON ?? existing.USU_CON,
                    USU_TEL = usuario.USU_TEL ?? existing.USU_TEL,
                    USU_EST = usuario.USU_EST ?? existing.USU_EST,
                    ROL_ID = usuario.ROL_ID ?? existing.ROL_ID,
                    // AGREGAMOS LA LÓGICA PARA EL ÁREA
                    ARE_ID = usuario.ARE_ID ?? existing.ARE_ID,
                    USU_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteUsuario(IResolverContext context, Guid usu_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.USUARIO WHERE USU_ID = @usu_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { usu_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}