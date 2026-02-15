using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class SolucionesServices
    {
        private readonly DataBase dataBase;
        private readonly SolucionesQueryBuilder solBuilder; // Inyección del Builder

        public SolucionesServices(DataBase dataBase, SolucionesQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.solBuilder = builder; // Asignación del builder
        }

        public async Task<IEnumerable<SolucionesModel>> GetSoluciones(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"s.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "s.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.SOLUCIONES s";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<SolucionesModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<SolucionesModel>> GetSolucionesByName(IResolverContext context, string nombre)
        {
            try
            {
                // Búsqueda por coincidencia en el nombre de la solución (SOL_NOM)
                string sqlQuery = "SELECT s.* FROM dbo.SOLUCIONES s WHERE s.SOL_NOM LIKE @nombre";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<SolucionesModel>(
                    sqlQuery,
                    new { nombre = $"%{nombre}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetSolucionesByName: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<SolucionesModel?> GetSolucionById(IResolverContext context, Guid sol_id)
        {
            try
            {
                string sqlQuery = "SELECT s.* FROM dbo.SOLUCIONES s WHERE s.SOL_ID = @sol_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<SolucionesModel>(sqlQuery, new { sol_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> InsertSolucion(IResolverContext context, SolucionesModel solucion)
        {
            try
            {
                if (solucion.SOL_ID == Guid.Empty) solucion.SOL_ID = Guid.NewGuid();
                if (solucion.SOL_FEC_CRE == default) solucion.SOL_FEC_CRE = DateTimeOffset.Now;

                string sqlQuery = @"INSERT INTO dbo.SOLUCIONES 
                                    (SOL_ID, SOL_NOM, SOL_DES, SOL_EST, SOL_TIE_RES, SOL_NIV_EFE, FRI_ID, USU_ID, SOL_FEC_CRE, SOL_FEC_MOD) 
                                    VALUES 
                                    (@SOL_ID, @SOL_NOM, @SOL_DES, @SOL_EST, @SOL_TIE_RES, @SOL_NIV_EFE, @FRI_ID, @USU_ID, @SOL_FEC_CRE, @SOL_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, solucion);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateSolucion(IResolverContext context, SolucionesModel solucion)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<SolucionesModel>(
                    "SELECT * FROM dbo.SOLUCIONES WHERE SOL_ID = @SOL_ID", new { solucion.SOL_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.SOLUCIONES SET 
                                    SOL_NOM = @SOL_NOM, SOL_DES = @SOL_DES, SOL_EST = @SOL_EST, 
                                    SOL_TIE_RES = @SOL_TIE_RES, SOL_NIV_EFE = @SOL_NIV_EFE, 
                                    FRI_ID = @FRI_ID, USU_ID = @USU_ID, SOL_FEC_MOD = @SOL_FEC_MOD 
                                    WHERE SOL_ID = @SOL_ID";

                var parameters = new
                {
                    SOL_ID = solucion.SOL_ID,
                    SOL_NOM = !string.IsNullOrEmpty(solucion.SOL_NOM) ? solucion.SOL_NOM : existing.SOL_NOM,
                    SOL_DES = !string.IsNullOrEmpty(solucion.SOL_DES) ? solucion.SOL_DES : existing.SOL_DES,
                    SOL_EST = !string.IsNullOrEmpty(solucion.SOL_EST) ? solucion.SOL_EST : existing.SOL_EST,
                    SOL_TIE_RES = solucion.SOL_TIE_RES ?? existing.SOL_TIE_RES,
                    SOL_NIV_EFE = solucion.SOL_NIV_EFE ?? existing.SOL_NIV_EFE,
                    FRI_ID = solucion.FRI_ID ?? existing.FRI_ID,
                    USU_ID = solucion.USU_ID ?? existing.USU_ID,
                    SOL_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteSolucion(IResolverContext context, Guid sol_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.SOLUCIONES WHERE SOL_ID = @sol_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { sol_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}