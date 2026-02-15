using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class BitacoraFriccionServices
    {
        private readonly DataBase dataBase;

        public BitacoraFriccionServices(DataBase dataBase, BitacoraFriccionQueryBuilder bifBuilder)
        {
            this.dataBase = dataBase;
        }

        // --- QUERIES ---

        public async Task<IEnumerable<BitacoraFriccionModel>> GetBitacoras(IResolverContext context)
        {
            try
            {
                // Mapeo dinámico: GraphQL (camelCase) -> SQL (BIT_FRI_...)
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<HotChocolate.Language.FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"b.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "b.*";
                // Tabla corregida a BITACORA_FRICCIONES
                string sqlQuery = $"SELECT {selectFields} FROM dbo.BITACORA_FRICCIONES b";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<BitacoraFriccionModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<BitacoraFriccionModel>> GetBitacorasByName(IResolverContext context, string nombre)
        {
            try
            {
                // SQL con LIKE para búsqueda por nombre
                string sqlQuery = "SELECT b.* FROM dbo.BITACORA_FRICCIONES b WHERE b.BIT_FRI_NOM LIKE @nombre";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<BitacoraFriccionModel>(
                    sqlQuery,
                    new { nombre = $"%{nombre}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetBitacorasByName: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<BitacoraFriccionModel?> GetBitacoraById(IResolverContext context, Guid bit_fri_id)
        {
            try
            {
                string sqlQuery = "SELECT b.* FROM dbo.BITACORA_FRICCIONES b WHERE b.BIT_FRI_ID = @bit_fri_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<BitacoraFriccionModel>(sqlQuery, new { bit_fri_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        // --- MUTATIONS ---

        public async Task<bool> InsertBitacora(IResolverContext context, BitacoraFriccionModel bitacora)
        {
            try
            {
                // Validación de IDs y Fechas con los nombres nuevos
                if (bitacora.BIT_FRI_ID == Guid.Empty) bitacora.BIT_FRI_ID = Guid.NewGuid();
                if (bitacora.BIT_FRI_FEC_CRE == default) bitacora.BIT_FRI_FEC_CRE = DateTimeOffset.Now;

                string sqlQuery = @"INSERT INTO dbo.BITACORA_FRICCIONES 
                    (BIT_FRI_ID, BIT_FRI_NOM, BIT_FRI_DES, BIT_FRI_EST, BIT_FRI_FEC_CRE, USU_ID, FRI_ID) 
                    VALUES 
                    (@BIT_FRI_ID, @BIT_FRI_NOM, @BIT_FRI_DES, @BIT_FRI_EST, @BIT_FRI_FEC_CRE, @USU_ID, @FRI_ID)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, bitacora);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateBitacora(IResolverContext context, BitacoraFriccionModel bitacora)
        {
            try
            {
                await dataBase.ConnectAsync();

                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<BitacoraFriccionModel>(
                    "SELECT * FROM dbo.BITACORA_FRICCIONES WHERE BIT_FRI_ID = @BIT_FRI_ID", new { bitacora.BIT_FRI_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.BITACORA_FRICCIONES SET 
                                    BIT_FRI_NOM = @BIT_FRI_NOM, 
                                    BIT_FRI_DES = @BIT_FRI_DES,
                                    BIT_FRI_EST = @BIT_FRI_EST,
                                    FRI_ID = @FRI_ID, 
                                    USU_ID = @USU_ID, 
                                    BIT_FRI_FEC_MOD = @BIT_FRI_FEC_MOD 
                                    WHERE BIT_FRI_ID = @BIT_FRI_ID";

                var parameters = new
                {
                    BIT_FRI_ID = bitacora.BIT_FRI_ID,
                    BIT_FRI_NOM = !string.IsNullOrEmpty(bitacora.BIT_FRI_NOM) ? bitacora.BIT_FRI_NOM : existing.BIT_FRI_NOM,
                    BIT_FRI_DES = !string.IsNullOrEmpty(bitacora.BIT_FRI_DES) ? bitacora.BIT_FRI_DES : existing.BIT_FRI_DES,
                    BIT_FRI_EST = bitacora.BIT_FRI_EST,
                    FRI_ID = bitacora.FRI_ID ?? existing.FRI_ID,
                    USU_ID = bitacora.USU_ID ?? existing.USU_ID,
                    BIT_FRI_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteBitacora(IResolverContext context, Guid bit_fri_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.BITACORA_FRICCIONES WHERE BIT_FRI_ID = @bit_fri_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { bit_fri_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}