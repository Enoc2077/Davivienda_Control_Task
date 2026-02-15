using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class PrioridadServices
    {
        private readonly DataBase dataBase;
        private readonly PrioridadQueryBuilder priBuilder; // Agregado el Builder

        public PrioridadServices(DataBase dataBase, PrioridadQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.priBuilder = builder; // Inyección del builder
        }

        public async Task<IEnumerable<PrioridadModel>> GetPrioridades(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"p.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "p.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.PRIORIDAD p";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<PrioridadModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<PrioridadModel>> GetPrioridadesByName(IResolverContext context, string nombre)
        {
            try
            {
                string sqlQuery = "SELECT p.* FROM dbo.PRIORIDAD p WHERE p.PRI_NOM LIKE @nombre";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<PrioridadModel>(
                    sqlQuery,
                    new { nombre = $"%{nombre}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetPrioridadesByName: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<PrioridadModel?> GetPrioridadById(IResolverContext context, Guid pri_id)
        {
            try
            {
                string sqlQuery = "SELECT p.* FROM dbo.PRIORIDAD p WHERE p.PRI_ID = @pri_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<PrioridadModel>(sqlQuery, new { pri_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> InsertPrioridad(IResolverContext context, PrioridadModel prioridad)
        {
            try
            {
                if (prioridad.PRI_ID == Guid.Empty) prioridad.PRI_ID = Guid.NewGuid();
                if (prioridad.PRI_FEC_CRE == default) prioridad.PRI_FEC_CRE = DateTimeOffset.Now;

                string sqlQuery = @"INSERT INTO dbo.PRIORIDAD 
                                    (PRI_ID, PRI_NOM, PRI_DES, PRI_NIV, PRI_FEC_CRE, PRI_FEC_MOD) 
                                    VALUES 
                                    (@PRI_ID, @PRI_NOM, @PRI_DES, @PRI_NIV, @PRI_FEC_CRE, @PRI_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, prioridad);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdatePrioridad(IResolverContext context, PrioridadModel prioridad)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<PrioridadModel>(
                    "SELECT * FROM dbo.PRIORIDAD WHERE PRI_ID = @PRI_ID", new { prioridad.PRI_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.PRIORIDAD SET 
                                    PRI_NOM = @PRI_NOM, PRI_DES = @PRI_DES, 
                                    PRI_NIV = @PRI_NIV, PRI_FEC_MOD = @PRI_FEC_MOD 
                                    WHERE PRI_ID = @PRI_ID";

                var parameters = new
                {
                    PRI_ID = prioridad.PRI_ID,
                    PRI_NOM = prioridad.PRI_NOM ?? existing.PRI_NOM,
                    PRI_DES = prioridad.PRI_DES ?? existing.PRI_DES,
                    PRI_NIV = prioridad.PRI_NIV != 0 ? prioridad.PRI_NIV : existing.PRI_NIV,
                    PRI_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeletePrioridad(IResolverContext context, Guid pri_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.PRIORIDAD WHERE PRI_ID = @pri_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { pri_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}