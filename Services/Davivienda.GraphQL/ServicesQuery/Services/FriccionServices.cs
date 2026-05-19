using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        #region QUERIES

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

                var bitacora = new BitacoraFriccionModel
                {
                    BIT_FRI_ID = Guid.NewGuid(),
                    BIT_FRI_NOM = $"Apertura: {friccion.FRI_TIP}",
                    BIT_FRI_DES = friccion.FRI_DES ?? "Registro inicial de fricción",
                    BIT_FRI_EST = true, 
                    BIT_FRI_FEC_CRE = friccion.FRI_FEC_CRE,
                    FRI_ID = friccion.FRI_ID,
                    USU_ID = friccion.USU_ID
                };

                await dataBase.ConnectAsync();

                using (var transaction = dataBase.Connection.BeginTransaction())
                {
                    try
                    {
                        
                        string sqlFri = @"INSERT INTO dbo.FRICCION 
                            (FRI_ID, FRI_TIP, FRI_DES, FRI_EST, FRI_IMP, TAR_ID, USU_ID, FRI_FEC_CRE) 
                            VALUES (@FRI_ID, @FRI_TIP, @FRI_DES, @FRI_EST, @FRI_IMP, @TAR_ID, @USU_ID, @FRI_FEC_CRE)";

                        await dataBase.Connection.ExecuteAsync(sqlFri, friccion, transaction);

                      
                        string sqlBit = @"INSERT INTO dbo.BITACORA_FRICCIONES 
                            (BIT_FRI_ID, BIT_FRI_NOM, BIT_FRI_DES, BIT_FRI_EST, BIT_FRI_FEC_CRE, FRI_ID, USU_ID) 
                            VALUES (@BIT_FRI_ID, @BIT_FRI_NOM, @BIT_FRI_DES, @BIT_FRI_EST, @BIT_FRI_FEC_CRE, @FRI_ID, @USU_ID)";

                        await dataBase.Connection.ExecuteAsync(sqlBit, bitacora, transaction);

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error en la transacción de Fricción: {ex.Message}");
                    }
                }
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateFriccion(IResolverContext context, FriccionModel friccion)
        {
            try
            {
                await dataBase.ConnectAsync();
                
                using (var transaction = dataBase.Connection.BeginTransaction())
                {
                    try
                    {
                        string sqlUpdate = @"UPDATE dbo.FRICCION SET 
                    FRI_TIP = @FRI_TIP, FRI_DES = @FRI_DES, FRI_EST = @FRI_EST, 
                    FRI_IMP = @FRI_IMP, FRI_FEC_MOD = @FRI_FEC_MOD 
                    WHERE FRI_ID = @FRI_ID";

                        await dataBase.Connection.ExecuteAsync(sqlUpdate, friccion, transaction);

                        var bitacora = new BitacoraFriccionModel
                        {
                            BIT_FRI_ID = Guid.NewGuid(),
                            BIT_FRI_NOM = $"Edición: {friccion.FRI_TIP}",
                            BIT_FRI_DES = $"Se actualizaron los detalles de la fricción. Estado actual: {friccion.FRI_EST}",
                            BIT_FRI_EST = true,
                            BIT_FRI_FEC_CRE = DateTimeOffset.Now,
                            FRI_ID = friccion.FRI_ID,
                            USU_ID = friccion.USU_ID
                        };

                        string sqlBit = @"INSERT INTO dbo.BITACORA_FRICCIONES 
                    (BIT_FRI_ID, BIT_FRI_NOM, BIT_FRI_DES, BIT_FRI_EST, BIT_FRI_FEC_CRE, FRI_ID, USU_ID) 
                    VALUES (@BIT_FRI_ID, @BIT_FRI_NOM, @BIT_FRI_DES, @BIT_FRI_EST, @BIT_FRI_FEC_CRE, @FRI_ID, @USU_ID)";

                        await dataBase.Connection.ExecuteAsync(sqlBit, bitacora, transaction);

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteFriccion(IResolverContext context, Guid fri_id)
        {
            try
            {
                await dataBase.ConnectAsync();

                string sqlDelBit = "DELETE FROM dbo.BITACORA_FRICCIONES WHERE FRI_ID = @fri_id";
                await dataBase.Connection.ExecuteAsync(sqlDelBit, new { fri_id });

                string sqlDelFri = "DELETE FROM dbo.FRICCION WHERE FRI_ID = @fri_id";
                var exec = await dataBase.Connection.ExecuteAsync(sqlDelFri, new { fri_id });

                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        #endregion
    }
}