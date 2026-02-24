using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.GraphQL.Security;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using Microsoft.Extensions.Configuration;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class UsuarioServices
    {
        private readonly DataBase dataBase;
        private readonly UsuarioQueryBuilder usuBuilder;
        private readonly JwtProvider jwtProvider; // Ya está inyectado aquí

        public UsuarioServices(DataBase dataBase, UsuarioQueryBuilder builder, JwtProvider jwtProvider)
        {
            this.dataBase = dataBase;
            this.usuBuilder = builder;
            this.jwtProvider = jwtProvider;
        }

        public async Task<LoginModel.LoginResponse> LoginAsync(LoginModel.LoginInput input)
        {
            try
            {
                await dataBase.ConnectAsync();

                // 🔥 JOIN con dbo.ROLES (plural)
                string sql = @"
            SELECT 
                u.USU_ID,
                u.USU_NOM,
                u.USU_NUM,
                u.USU_COR,
                u.USU_CON,
                u.USU_TEL,
                u.USU_EST,
                u.ROL_ID,
                u.ARE_ID,
                u.USU_FEC_CRE,
                u.USU_FEC_MOD,
                r.ROL_NOM
            FROM dbo.USUARIO u
            INNER JOIN dbo.ROLES r ON u.ROL_ID = r.ROL_ID
            WHERE u.USU_NUM = @UsuNum 
            AND u.USU_CON = @Password 
            AND u.USU_EST = 1";

                var resultado = await dataBase.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
                {
                    UsuNum = input.UsuNum,
                    Password = input.Password
                });

                if (resultado == null)
                {
                    return new LoginModel.LoginResponse
                    {
                        Exito = false,
                        Mensaje = "Número de empleado o contraseña incorrectos."
                    };
                }

                string rolNombre = resultado.ROL_NOM ?? "Usuario";

                string token = jwtProvider.GenerarToken(
                    int.TryParse(resultado.USU_NUM, out int num) ? num : 0,
                    resultado.USU_NOM ?? "Usuario",
                    rolNombre  // ✅ Pasará "Enoc"
                );

                var usuario = new UsuarioModel
                {
                    USU_ID = resultado.USU_ID,
                    USU_NOM = resultado.USU_NOM,
                    USU_NUM = resultado.USU_NUM,
                    USU_COR = resultado.USU_COR,
                    USU_TEL = resultado.USU_TEL,
                    USU_EST = resultado.USU_EST,
                    ROL_ID = resultado.ROL_ID,
                    ARE_ID = resultado.ARE_ID,
                    USU_FEC_CRE = resultado.USU_FEC_CRE,
                    USU_FEC_MOD = resultado.USU_FEC_MOD
                };

                return new LoginModel.LoginResponse
                {
                    Exito = true,
                    Token = token,
                    Mensaje = "Bienvenido al sistema",
                    Usuario = usuario
                };
            }
            catch (Exception ex)
            {
                return new LoginModel.LoginResponse
                {
                    Exito = false,
                    Mensaje = $"Error en el servidor: {ex.Message}"
                };
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }







        // --- CONSULTAS EXISTENTES ---

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
                await dataBase.ConnectAsync();
                // 1. Primero ponemos en NULL o borramos las tareas del usuario
                string sqlTareas = "UPDATE dbo.TAREA SET USU_ID = NULL WHERE USU_ID = @usu_id";
                await dataBase.Connection.ExecuteAsync(sqlTareas, new { usu_id });

                // 2. Ahora sí borramos al usuario
                string sqlUsuario = "DELETE FROM dbo.USUARIO WHERE USU_ID = @usu_id";
                var exec = await dataBase.Connection.ExecuteAsync(sqlUsuario, new { usu_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}