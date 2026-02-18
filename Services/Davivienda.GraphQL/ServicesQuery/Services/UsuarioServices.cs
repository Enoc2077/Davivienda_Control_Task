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

        // --- MÉTODO DE LOGIN PRINCIPAL (CORREGIDO) ---
        public async Task<LoginModel.LoginResponse> LoginAsync(LoginModel.LoginInput input)
        {
            try
            {
                await dataBase.ConnectAsync();

                // 1. Buscamos al usuario por número de empleado y contraseña (USU_CON)
                // Nota: Asegúrate de que el nombre de la tabla sea USUARIO o USUARIOS según tu DB
                string sql = @"SELECT * FROM dbo.USUARIO 
                               WHERE USU_NUM = @UsuNum 
                               AND USU_CON = @Password 
                               AND USU_EST = 1";

                var usuario = await dataBase.Connection.QueryFirstOrDefaultAsync<UsuarioModel>(sql, new
                {
                    UsuNum = input.UsuNum,
                    Password = input.Password
                });

                if (usuario == null)
                {
                    return new LoginModel.LoginResponse
                    {
                        Exito = false,
                        Mensaje = "Número de empleado o contraseña incorrectos."
                    };
                }

                // 2. Generar el Token JWT usando la instancia inyectada (jwtProvider)
                // Convertimos el USU_NUM (string) a int para el token
                string token = jwtProvider.GenerarToken(
                    int.TryParse(usuario.USU_NUM, out int num) ? num : 0,
                    usuario.USU_NOM ?? "Usuario",
                    0 // Aquí puedes poner un ID de rol si lo manejas numérico
                );

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