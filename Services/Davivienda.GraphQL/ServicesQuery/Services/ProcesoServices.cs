using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class ProcesoServices
    {
        private readonly DataBase dataBase;
        private readonly ProcesoQueryBuilder proBuilder; // Inyectamos el Builder

        public ProcesoServices(DataBase dataBase, ProcesoQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.proBuilder = builder; // Asignamos el builder inyectado
        }

        public async Task<IEnumerable<ProcesoModel>> GetProcesos(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"pr.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "pr.*";
                // Aseguramos el nombre en singular: PROCESO
                string sqlQuery = $"SELECT {selectFields} FROM dbo.PROCESO pr";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<ProcesoModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<ProcesoModel>> GetProcesosByName(IResolverContext context, string nombre)
        {
            try
            {
                // Búsqueda por coincidencia en PROC_NOM
                string sqlQuery = "SELECT pr.* FROM dbo.PROCESO pr WHERE pr.PROC_NOM LIKE @nombre";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<ProcesoModel>(
                    sqlQuery,
                    new { nombre = $"%{nombre}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetProcesosByName: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<ProcesoModel?> GetProcesoById(IResolverContext context, Guid proc_id)
        {
            try
            {
                string sqlQuery = "SELECT pr.* FROM dbo.PROCESO pr WHERE pr.PROC_ID = @proc_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<ProcesoModel>(sqlQuery, new { proc_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> InsertProceso(IResolverContext context, ProcesoModel proceso)
        {
            try
            {
                if (proceso.PROC_ID == Guid.Empty) proceso.PROC_ID = Guid.NewGuid();
                if (proceso.PROC_FEC_CRE == default) proceso.PROC_FEC_CRE = DateTimeOffset.Now;

                string sqlQuery = @"INSERT INTO dbo.PROCESO 
                                    (PROC_ID, PROC_NOM, PROC_DES, PROC_FRE, PROC_EST, PRO_ID, PROC_FEC_CRE, PROC_FEC_MOD) 
                                    VALUES 
                                    (@PROC_ID, @PROC_NOM, @PROC_DES, @PROC_FRE, @PROC_EST, @PRO_ID, @PROC_FEC_CRE, @PROC_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, proceso);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateProceso(IResolverContext context, ProcesoModel proceso)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<ProcesoModel>(
                    "SELECT * FROM dbo.PROCESO WHERE PROC_ID = @PROC_ID", new { proceso.PROC_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.PROCESO SET 
                                    PROC_NOM = @PROC_NOM, PROC_DES = @PROC_DES, PROC_FRE = @PROC_FRE, 
                                    PROC_EST = @PROC_EST, PRO_ID = @PRO_ID, PROC_FEC_MOD = @PROC_FEC_MOD 
                                    WHERE PROC_ID = @PROC_ID";

                var parameters = new
                {
                    PROC_ID = proceso.PROC_ID,
                    PROC_NOM = !string.IsNullOrEmpty(proceso.PROC_NOM) ? proceso.PROC_NOM : existing.PROC_NOM,
                    PROC_DES = proceso.PROC_DES ?? existing.PROC_DES,
                    PROC_FRE = proceso.PROC_FRE ?? existing.PROC_FRE,
                    PROC_EST = proceso.PROC_EST ?? existing.PROC_EST,
                    PRO_ID = proceso.PRO_ID ?? existing.PRO_ID,
                    PROC_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteProceso(IResolverContext context, Guid proc_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.PROCESO WHERE PROC_ID = @proc_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { proc_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}