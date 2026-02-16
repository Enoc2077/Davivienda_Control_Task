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
    public class SolucionesServices
    {
        private readonly DataBase dataBase;
        private readonly SolucionesQueryBuilder solBuilder;

        public SolucionesServices(DataBase dataBase, SolucionesQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.solBuilder = builder;
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
                string sqlQuery = "SELECT s.* FROM dbo.SOLUCIONES s WHERE s.SOL_NOM LIKE @nombre";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<SolucionesModel>(sqlQuery, new { nombre = $"%{nombre}%" });
            }
            finally { await dataBase.DisconnectAsync(); }
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
                // 1. Preparación de datos
                if (solucion.SOL_ID == Guid.Empty) solucion.SOL_ID = Guid.NewGuid();
                if (solucion.SOL_FEC_CRE == default) solucion.SOL_FEC_CRE = DateTimeOffset.Now;

                var bitacora = new BitacoraSolucionesModel
                {
                    BIT_SOL_ID = Guid.NewGuid(),
                    BIT_SOL_NOM = solucion.SOL_NOM,
                    BIT_SOL_EST = solucion.SOL_EST,
                    BIT_SOL_DES = solucion.SOL_DES,
                    BIT_SOL_TIE_TOT_TRA = DateTimeOffset.Now,
                    SOL_ID = solucion.SOL_ID,
                    USU_ID = solucion.USU_ID,
                    BIT_SOL_FEC_CRE = solucion.SOL_FEC_CRE
                };

                // 2. Conexión y Limpieza
                await dataBase.ConnectAsync();

                // 3. Bloque Transaccional
                using (var transaction = dataBase.Connection.BeginTransaction())
                {
                    try
                    {
                        string sqlSol = @"INSERT INTO dbo.SOLUCIONES 
                    (SOL_ID, SOL_NOM, SOL_DES, SOL_EST, SOL_TIE_RES, SOL_NIV_EFE, FRI_ID, USU_ID, SOL_FEC_CRE) 
                    VALUES (@SOL_ID, @SOL_NOM, @SOL_DES, @SOL_EST, @SOL_TIE_RES, @SOL_NIV_EFE, @FRI_ID, @USU_ID, @SOL_FEC_CRE)";

                        // IMPORTANTE: Pasar siempre el objeto 'transaction'
                        await dataBase.Connection.ExecuteAsync(sqlSol, solucion, transaction);

                        string sqlBit = @"INSERT INTO dbo.BITACORA_SOLUCIONES 
                    (BIT_SOL_ID, BIT_SOL_NOM, BIT_SOL_EST, BIT_SOL_DES, BIT_SOL_TIE_TOT_TRA, SOL_ID, USU_ID, BIT_SOL_FEC_CRE) 
                    VALUES (@BIT_SOL_ID, @BIT_SOL_NOM, @BIT_SOL_EST, @BIT_SOL_DES, @BIT_SOL_TIE_TOT_TRA, @SOL_ID, @USU_ID, @BIT_SOL_FEC_CRE)";

                        await dataBase.Connection.ExecuteAsync(sqlBit, bitacora, transaction);

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error en transacción SQL: {ex.Message}");
                    }
                }
            }
            finally
            {
                // Cerramos físicamente la conexión para liberar el hilo en el pool
                await dataBase.DisconnectAsync();
            }
        }



        public async Task<bool> UpdateSolucion(IResolverContext context, SolucionesModel solucion)
        {
            try
            {
                await dataBase.ConnectAsync();
                using (var transaction = dataBase.Connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Actualizar Tabla Principal
                        string sqlUpdate = @"UPDATE dbo.SOLUCIONES SET 
                    SOL_NOM = @SOL_NOM, SOL_DES = @SOL_DES, SOL_EST = @SOL_EST, 
                    SOL_NIV_EFE = @SOL_NIV_EFE, SOL_FEC_MOD = @SOL_FEC_MOD 
                    WHERE SOL_ID = @SOL_ID";

                        await dataBase.Connection.ExecuteAsync(sqlUpdate, solucion, transaction);

                        // 2. Insertar Nuevo Registro en Bitácora (Auditoría de cambio)
                        var bitacora = new BitacoraSolucionesModel
                        {
                            BIT_SOL_ID = Guid.NewGuid(),
                            BIT_SOL_NOM = $"Modificación: {solucion.SOL_NOM}",
                            BIT_SOL_EST = solucion.SOL_EST,
                            BIT_SOL_DES = $"Cambio realizado: {solucion.SOL_DES}",
                            BIT_SOL_TIE_TOT_TRA = DateTimeOffset.Now,
                            SOL_ID = solucion.SOL_ID,
                            USU_ID = solucion.USU_ID,
                            BIT_SOL_FEC_CRE = DateTimeOffset.Now
                        };

                        string sqlBit = @"INSERT INTO dbo.BITACORA_SOLUCIONES 
                    (BIT_SOL_ID, BIT_SOL_NOM, BIT_SOL_EST, BIT_SOL_DES, BIT_SOL_TIE_TOT_TRA, SOL_ID, USU_ID, BIT_SOL_FEC_CRE) 
                    VALUES (@BIT_SOL_ID, @BIT_SOL_NOM, @BIT_SOL_EST, @BIT_SOL_DES, @BIT_SOL_TIE_TOT_TRA, @SOL_ID, @USU_ID, @BIT_SOL_FEC_CRE)";

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

        public async Task<bool> DeleteSolucion(IResolverContext context, Guid sol_id)
        {
            try
            {
                // NOTA: Si BITACORA_SOLUCIONES tiene una FK hacia SOLUCIONES, 
                // primero deberías borrar la bitácora o usar ON DELETE CASCADE en SQL.
                string sqlQuery = "DELETE FROM dbo.SOLUCIONES WHERE SOL_ID = @sol_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { sol_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}