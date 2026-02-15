using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class FriccionServices
    {
        private readonly DataBase dataBase;
        private readonly FriccionQueryBuilder friBuilder;

        public FriccionServices(DataBase dataBase, FriccionQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.friBuilder = builder;
        }

        public async Task<IEnumerable<FriccionModel>> GetFricciones(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"f.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "f.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.FRICCION f";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<FriccionModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }


        public async Task<IEnumerable<FriccionModel>> GetFriccionByDescription(IResolverContext context, string descripcion)
        {
            try
            {
                // Buscamos coincidencias en la descripción de la fricción (FRI_DES)
                string sqlQuery = "SELECT f.* FROM dbo.FRICCION f WHERE f.FRI_DES LIKE @descripcion";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<FriccionModel>(
                    sqlQuery,
                    new { descripcion = $"%{descripcion}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetFriccionByDescription: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<FriccionModel?> GetFriccionById(IResolverContext context, Guid fri_id)
        {
            try
            {
                string sqlQuery = "SELECT f.* FROM dbo.FRICCION f WHERE f.FRI_ID = @fri_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<FriccionModel>(sqlQuery, new { fri_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> InsertFriccion(IResolverContext context, FriccionModel friccion)
        {
            try
            {
                if (friccion.FRI_ID == Guid.Empty) friccion.FRI_ID = Guid.NewGuid();
                if (friccion.FRI_FEC_CRE == default) friccion.FRI_FEC_CRE = DateTimeOffset.Now;

                string sqlQuery = @"INSERT INTO dbo.FRICCION 
                                    (FRI_ID, FRI_TIP, FRI_DES, FRI_EST, FRI_IMP, TAR_ID, USU_ID, FRI_FEC_CRE, FRI_FEC_MOD) 
                                    VALUES 
                                    (@FRI_ID, @FRI_TIP, @FRI_DES, @FRI_EST, @FRI_IMP, @TAR_ID, @USU_ID, @FRI_FEC_CRE, @FRI_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, friccion);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateFriccion(IResolverContext context, FriccionModel friccion)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<FriccionModel>(
                    "SELECT * FROM dbo.FRICCION WHERE FRI_ID = @FRI_ID", new { friccion.FRI_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.FRICCION SET 
                                    FRI_TIP = @FRI_TIP, FRI_DES = @FRI_DES, FRI_EST = @FRI_EST, 
                                    FRI_IMP = @FRI_IMP, TAR_ID = @TAR_ID, USU_ID = @USU_ID, 
                                    FRI_FEC_MOD = @FRI_FEC_MOD WHERE FRI_ID = @FRI_ID";

                var parameters = new
                {
                    FRI_ID = friccion.FRI_ID,
                    FRI_TIP = friccion.FRI_TIP ?? existing.FRI_TIP,
                    FRI_DES = friccion.FRI_DES ?? existing.FRI_DES,
                    FRI_EST = friccion.FRI_EST ?? existing.FRI_EST,
                    FRI_IMP = friccion.FRI_IMP ?? existing.FRI_IMP,
                    TAR_ID = friccion.TAR_ID ?? existing.TAR_ID,
                    USU_ID = friccion.USU_ID ?? existing.USU_ID,
                    FRI_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteFriccion(IResolverContext context, Guid fri_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.FRICCION WHERE FRI_ID = @fri_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { fri_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}