using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class NotificacionesServices
    {
        private readonly DataBase dataBase;
        private readonly NotificacionesQueryBuilder notBuilder;

        public NotificacionesServices(DataBase dataBase, NotificacionesQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.notBuilder = builder;
        }

        public async Task<IEnumerable<NotificacionesModel>> GetNotificaciones(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"n.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "n.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.NOTIFICACIONES n";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<NotificacionesModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<NotificacionesModel>> GetNotificacionesByMessage(IResolverContext context, string mensaje)
        {
            try
            {
                // Búsqueda por coincidencia en el cuerpo del mensaje de la notificación
                string sqlQuery = "SELECT n.* FROM dbo.NOTIFICACIONES n WHERE n.NOT_MEN LIKE @mensaje";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<NotificacionesModel>(
                    sqlQuery,
                    new { mensaje = $"%{mensaje}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetNotificacionesByMessage: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<NotificacionesModel?> GetNotificacionById(IResolverContext context, Guid not_id)
        {
            try
            {
                string sqlQuery = "SELECT n.* FROM dbo.NOTIFICACIONES n WHERE n.NOT_ID = @not_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<NotificacionesModel>(sqlQuery, new { not_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> InsertNotificacion(IResolverContext context, NotificacionesModel notificacion)
        {
            try
            {
                if (notificacion.NOT_ID == Guid.Empty) notificacion.NOT_ID = Guid.NewGuid();
                if (notificacion.NOT_FEC_CRE == default) notificacion.NOT_FEC_CRE = DateTimeOffset.Now;

                string sqlQuery = @"INSERT INTO dbo.NOTIFICACIONES 
                                    (NOT_ID, NOT_MEN, NOT_LEI, USU_ID, NOT_FEC_CRE, NOT_FEC_MOD) 
                                    VALUES 
                                    (@NOT_ID, @NOT_MEN, @NOT_LEI, @USU_ID, @NOT_FEC_CRE, @NOT_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, notificacion);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateNotificacion(IResolverContext context, NotificacionesModel notificacion)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<NotificacionesModel>(
                    "SELECT * FROM dbo.NOTIFICACIONES WHERE NOT_ID = @NOT_ID", new { notificacion.NOT_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.NOTIFICACIONES SET 
                                    NOT_MEN = @NOT_MEN, NOT_LEI = @NOT_LEI, USU_ID = @USU_ID, 
                                    NOT_FEC_MOD = @NOT_FEC_MOD WHERE NOT_ID = @NOT_ID";

                var parameters = new
                {
                    NOT_ID = notificacion.NOT_ID,
                    NOT_MEN = !string.IsNullOrEmpty(notificacion.NOT_MEN) ? notificacion.NOT_MEN : existing.NOT_MEN,
                    NOT_LEI = notificacion.NOT_LEI, // Al ser bool, se actualiza directo
                    USU_ID = notificacion.USU_ID ?? existing.USU_ID,
                    NOT_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteNotificacion(IResolverContext context, Guid not_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.NOTIFICACIONES WHERE NOT_ID = @not_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { not_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}