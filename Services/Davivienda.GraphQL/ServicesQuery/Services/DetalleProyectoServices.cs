using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class DetalleProyectoServices
    {
        private readonly DataBase dataBase;
        private readonly DetalleProyectoQueryBuilder detProBuilder;

        public DetalleProyectoServices(DataBase dataBase, DetalleProyectoQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.detProBuilder = builder;
        }

        public async Task<IEnumerable<DetalleProyectoModel>> GetDetallesProyecto(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"dp.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "dp.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.DETALLE_PROYECTO dp";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<DetalleProyectoModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<DetalleProyectoModel>> GetDetallesByProyecto(IResolverContext context, Guid proyectoId)
        {
            try
            {
                string sqlQuery = "SELECT dp.* FROM dbo.DETALLE_PROYECTO dp WHERE dp.PRO_ID = @proyectoId";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<DetalleProyectoModel>(sqlQuery, new { proyectoId });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetDetallesByProyecto: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<DetalleProyectoModel?> GetDetalleProyectoById(IResolverContext context, Guid det_pro_id)
        {
            try
            {
                string sqlQuery = "SELECT dp.* FROM dbo.DETALLE_PROYECTO dp WHERE dp.DET_PRO_ID = @det_pro_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<DetalleProyectoModel>(sqlQuery, new { det_pro_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> InsertDetalleProyecto(IResolverContext context, DetalleProyectoModel detalle)
        {
            try
            {
                if (detalle.DET_PRO_ID == Guid.Empty) detalle.DET_PRO_ID = Guid.NewGuid();
                if (detalle.DET_PRO_FEC_CRE == default) detalle.DET_PRO_FEC_CRE = DateTimeOffset.Now;
                if (detalle.DET_PRO_FEC_ASI == default) detalle.DET_PRO_FEC_ASI = DateTimeOffset.Now;

                string sqlQuery = @"INSERT INTO dbo.DETALLE_PROYECTO 
                                    (DET_PRO_ID, PRO_ID, USU_ID, ROL_ID, DET_PRO_FEC_ASI, DET_PRO_FEC_CRE, DET_PRO_FEC_MOD) 
                                    VALUES 
                                    (@DET_PRO_ID, @PRO_ID, @USU_ID, @ROL_ID, @DET_PRO_FEC_ASI, @DET_PRO_FEC_CRE, @DET_PRO_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, detalle);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateDetalleProyecto(IResolverContext context, DetalleProyectoModel detalle)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<DetalleProyectoModel>(
                    "SELECT * FROM dbo.DETALLE_PROYECTO WHERE DET_PRO_ID = @DET_PRO_ID", new { detalle.DET_PRO_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.DETALLE_PROYECTO SET 
                                    PRO_ID = @PRO_ID, USU_ID = @USU_ID, ROL_ID = @ROL_ID, 
                                    DET_PRO_FEC_ASI = @DET_PRO_FEC_ASI, DET_PRO_FEC_MOD = @DET_PRO_FEC_MOD 
                                    WHERE DET_PRO_ID = @DET_PRO_ID";

                var parameters = new
                {
                    DET_PRO_ID = detalle.DET_PRO_ID,
                    PRO_ID = detalle.PRO_ID ?? existing.PRO_ID,
                    USU_ID = detalle.USU_ID ?? existing.USU_ID,
                    ROL_ID = detalle.ROL_ID ?? existing.ROL_ID,
                    DET_PRO_FEC_ASI = detalle.DET_PRO_FEC_ASI == default ? existing.DET_PRO_FEC_ASI : detalle.DET_PRO_FEC_ASI,
                    DET_PRO_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteDetalleProyecto(IResolverContext context, Guid det_pro_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.DETALLE_PROYECTO WHERE DET_PRO_ID = @det_pro_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { det_pro_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}