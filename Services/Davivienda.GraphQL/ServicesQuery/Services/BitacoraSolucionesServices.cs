using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class BitacoraSolucionesServices
    {
        private readonly DataBase dataBase;
        private readonly BitacoraSolucionesQueryBuilder bitSolBuilder;

        public BitacoraSolucionesServices(DataBase dataBase, BitacoraSolucionesQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.bitSolBuilder = builder;
        }

        public async Task<IEnumerable<BitacoraSolucionesModel>> GetBitacoraSoluciones(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"bs.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "bs.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.BITACORA_SOLUCIONES bs";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<BitacoraSolucionesModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<BitacoraSolucionesModel>> GetBitacoraSolucionesByName(IResolverContext context, string nombre)
        {
            try
            {
                // Búsqueda por coincidencia en BIT_SOL_NOM
                string sqlQuery = "SELECT bs.* FROM dbo.BITACORA_SOLUCIONES bs WHERE bs.BIT_SOL_NOM LIKE @nombre";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<BitacoraSolucionesModel>(
                    sqlQuery,
                    new { nombre = $"%{nombre}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetBitacoraSolucionesByName: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }


        public async Task<BitacoraSolucionesModel?> GetBitacoraSolucionById(IResolverContext context, Guid bit_sol_id)
        {
            try
            {
                string sqlQuery = "SELECT bs.* FROM dbo.BITACORA_SOLUCIONES bs WHERE bs.BIT_SOL_ID = @bit_sol_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<BitacoraSolucionesModel>(sqlQuery, new { bit_sol_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }







        public async Task<bool> InsertBitacoraSolucion(IResolverContext context, BitacoraSolucionesModel bitacora)
        {
            try
            {
                if (bitacora.BIT_SOL_ID == Guid.Empty) bitacora.BIT_SOL_ID = Guid.NewGuid();
                if (bitacora.BIT_SOL_FEC_CRE == default) bitacora.BIT_SOL_FEC_CRE = DateTimeOffset.Now;

                string sqlQuery = @"INSERT INTO dbo.BITACORA_SOLUCIONES 
                                    (BIT_SOL_ID, BIT_SOL_NOM, BIT_SOL_EST, BIT_SOL_DES, BIT_SOL_TIE_TOT_TRA, SOL_ID, USU_ID, BIT_SOL_FEC_CRE, BIT_SOL_FEC_MOD) 
                                    VALUES 
                                    (@BIT_SOL_ID, @BIT_SOL_NOM, @BIT_SOL_EST, @BIT_SOL_DES, @BIT_SOL_TIE_TOT_TRA, @SOL_ID, @USU_ID, @BIT_SOL_FEC_CRE, @BIT_SOL_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, bitacora);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateBitacoraSolucion(IResolverContext context, BitacoraSolucionesModel bitacora)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<BitacoraSolucionesModel>(
                    "SELECT * FROM dbo.BITACORA_SOLUCIONES WHERE BIT_SOL_ID = @BIT_SOL_ID", new { bitacora.BIT_SOL_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.BITACORA_SOLUCIONES SET 
                                    BIT_SOL_NOM = @BIT_SOL_NOM, BIT_SOL_EST = @BIT_SOL_EST, BIT_SOL_DES = @BIT_SOL_DES, 
                                    BIT_SOL_TIE_TOT_TRA = @BIT_SOL_TIE_TOT_TRA, SOL_ID = @SOL_ID, USU_ID = @USU_ID, 
                                    BIT_SOL_FEC_MOD = @BIT_SOL_FEC_MOD WHERE BIT_SOL_ID = @BIT_SOL_ID";

                var parameters = new
                {
                    BIT_SOL_ID = bitacora.BIT_SOL_ID,
                    BIT_SOL_NOM = bitacora.BIT_SOL_NOM ?? existing.BIT_SOL_NOM,
                    BIT_SOL_EST = bitacora.BIT_SOL_EST ?? existing.BIT_SOL_EST,
                    BIT_SOL_DES = bitacora.BIT_SOL_DES ?? existing.BIT_SOL_DES,
                    BIT_SOL_TIE_TOT_TRA = bitacora.BIT_SOL_TIE_TOT_TRA == default ? existing.BIT_SOL_TIE_TOT_TRA : bitacora.BIT_SOL_TIE_TOT_TRA,
                    SOL_ID = bitacora.SOL_ID ?? existing.SOL_ID,
                    USU_ID = bitacora.USU_ID ?? existing.USU_ID,
                    BIT_SOL_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteBitacoraSolucion(IResolverContext context, Guid bit_sol_id)
        {
            try
            {
                // SQL Manual para asegurar que el borrado apunte a la tabla correcta
                string sqlQuery = "DELETE FROM dbo.BITACORA_SOLUCIONES WHERE BIT_SOL_ID = @bit_sol_id";

                await dataBase.ConnectAsync();

                // Ejecutamos y retornamos true si se eliminó al menos 1 fila
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { bit_sol_id });
                return exec > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar en BitacoraSoluciones: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }
    }
}