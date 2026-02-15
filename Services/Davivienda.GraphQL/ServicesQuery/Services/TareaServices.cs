using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class TareaServices
    {
        private readonly DataBase dataBase;
        private readonly TareaQueryBuilder tarBuilder; // Inyección del Builder

        public TareaServices(DataBase dataBase, TareaQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.tarBuilder = builder; // Asignación del builder
        }

        public async Task<IEnumerable<TareaModel>> GetTareas(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"t.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "t.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.TAREA t";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<TareaModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<TareaModel>> GetTareasByName(IResolverContext context, string nombre)
        {
            try
            {
                // Búsqueda por coincidencia en TAR_NOM
                string sqlQuery = "SELECT t.* FROM dbo.TAREA t WHERE t.TAR_NOM LIKE @nombre";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<TareaModel>(
                    sqlQuery,
                    new { nombre = $"%{nombre}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetTareasByName: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }


        public async Task<TareaModel?> GetTareaById(IResolverContext context, Guid tar_id)
        {
            try
            {
                string sqlQuery = "SELECT t.* FROM dbo.TAREA t WHERE t.TAR_ID = @tar_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<TareaModel>(sqlQuery, new { tar_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> InsertTarea(IResolverContext context, TareaModel tarea)
        {
            try
            {
                if (tarea.TAR_ID == Guid.Empty) tarea.TAR_ID = Guid.NewGuid();
                if (tarea.TAR_FEC_CRE == default) tarea.TAR_FEC_CRE = DateTimeOffset.Now;
                if (tarea.TAR_FEC_INI == default) tarea.TAR_FEC_INI = DateTimeOffset.Now;

                string sqlQuery = @"INSERT INTO dbo.TAREA 
                                    (TAR_ID, TAR_NOM, TAR_DES, TAR_EST, TAR_FEC_INI, TAR_FEC_FIN, PROC_ID, PRI_ID, USU_ID, TAR_FEC_CRE, TAR_FEC_MOD) 
                                    VALUES 
                                    (@TAR_ID, @TAR_NOM, @TAR_DES, @TAR_EST, @TAR_FEC_INI, @TAR_FEC_FIN, @PROC_ID, @PRI_ID, @USU_ID, @TAR_FEC_CRE, @TAR_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, tarea);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateTarea(IResolverContext context, TareaModel tarea)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<TareaModel>(
                    "SELECT * FROM dbo.TAREA WHERE TAR_ID = @TAR_ID", new { tarea.TAR_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.TAREA SET 
                                    TAR_NOM = @TAR_NOM, TAR_DES = @TAR_DES, TAR_EST = @TAR_EST, 
                                    TAR_FEC_INI = @TAR_FEC_INI, TAR_FEC_FIN = @TAR_FEC_FIN, 
                                    PROC_ID = @PROC_ID, PRI_ID = @PRI_ID, USU_ID = @USU_ID, 
                                    TAR_FEC_MOD = @TAR_FEC_MOD WHERE TAR_ID = @TAR_ID";

                var parameters = new
                {
                    TAR_ID = tarea.TAR_ID,
                    TAR_NOM = !string.IsNullOrEmpty(tarea.TAR_NOM) ? tarea.TAR_NOM : existing.TAR_NOM,
                    TAR_DES = tarea.TAR_DES ?? existing.TAR_DES,
                    TAR_EST = tarea.TAR_EST ?? existing.TAR_EST,
                    TAR_FEC_INI = tarea.TAR_FEC_INI == default ? existing.TAR_FEC_INI : tarea.TAR_FEC_INI,
                    TAR_FEC_FIN = tarea.TAR_FEC_FIN ?? existing.TAR_FEC_FIN,
                    PROC_ID = tarea.PROC_ID ?? existing.PROC_ID,
                    PRI_ID = tarea.PRI_ID ?? existing.PRI_ID,
                    USU_ID = tarea.USU_ID ?? existing.USU_ID,
                    TAR_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteTarea(IResolverContext context, Guid tar_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.TAREA WHERE TAR_ID = @tar_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { tar_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}