using Dapper.GraphQL;
using Davivienda.Models;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Resolvers;
using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.GraphQL.Extensions;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class AreaServices
    {
        private readonly DataBase dataBase;
        private readonly AreaQueryBuilder areaQueryBuilder;

        public AreaServices(DataBase dataBase, AreaQueryBuilder areaBuilder)
        {
            this.dataBase = dataBase;
            this.areaQueryBuilder = areaBuilder;
        }

        // --- QUERIS ---

        public async Task<IEnumerable<AreasModel>> GetAreas(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = new List<string>();

                foreach (var selection in selections)
                {
                    if (selection is HotChocolate.Language.FieldNode fieldNode)
                    {
                        string fieldName = fieldNode.Name.Value;
                        if (fieldName != "__typename")
                        {
                            fields.Add($"a.{fieldName.ToUpper()}");
                        }
                    }
                }

                string selectFields = fields.Any() ? string.Join(", ", fields) : "a.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.AREA a";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<AreasModel>(sqlQuery);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<IEnumerable<AreasModel>> GetAreasByName(IResolverContext context, string area_name)
        {
            try
            {
                
                string sqlQuery = "SELECT a.* FROM dbo.AREA a WHERE a.ARE_NOM LIKE @area_name";

                System.Diagnostics.Debug.WriteLine($"SQL DEBUG: {sqlQuery}");

                await dataBase.ConnectAsync();

                var result = await dataBase.Connection.QueryAsync<AreasModel>(
                    sqlQuery,
                    new { area_name = $"%{area_name}%" }
                );

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetAreasByName: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<AreasModel?> GetAreaById(IResolverContext context, Guid area_id)
        {
            try
            {
                string sqlQuery = "SELECT a.* FROM dbo.AREA a WHERE a.ARE_ID = @area_id";

                await dataBase.ConnectAsync();

                var result = await dataBase.Connection.QueryFirstOrDefaultAsync<AreasModel>(
                    sqlQuery,
                    new { area_id }
                );

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetAreaById: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        // --- MUTATIONS ---

        public async Task<bool> InsertArea(IResolverContext context, AreasModel area)
        {
            try
            {
                if (area.ARE_ID == Guid.Empty) area.ARE_ID = Guid.NewGuid();

                string sqlQuery = @"INSERT INTO dbo.AREA 
                                    (ARE_ID, ARE_NOM, ARE_DES, ARE_EST, ARE_FEC_CRE, ARE_FEC_MOD) 
                                    VALUES 
                                    (@ARE_ID, @ARE_NOM, @ARE_DES, @ARE_EST, @ARE_FEC_CRE, @ARE_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, area);
                return exec > 0;
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<bool> UpdateAreas(IResolverContext context, AreasModel area)
        {
            try
            {
                await dataBase.ConnectAsync();

                var existingArea = await dataBase.Connection.QueryFirstOrDefaultAsync<AreasModel>(
                    "SELECT * FROM dbo.AREA WHERE ARE_ID = @ARE_ID", new { area.ARE_ID });

                if (existingArea == null) return false;

                string sqlQuery = @"UPDATE dbo.AREA SET 
                            ARE_NOM = @ARE_NOM, 
                            ARE_DES = @ARE_DES, 
                            ARE_EST = @ARE_EST, 
                            ARE_FEC_MOD = @ARE_FEC_MOD 
                            WHERE ARE_ID = @ARE_ID";

                var parameters = new
                {
                    ARE_ID = area.ARE_ID,
                    ARE_NOM = area.ARE_NOM ?? existingArea.ARE_NOM,
                    ARE_DES = area.ARE_DES ?? existingArea.ARE_DES,
                    ARE_EST = area.ARE_EST ?? existingArea.ARE_EST, 
                    ARE_FEC_MOD = DateTime.Now 
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<bool> DeleteAreas(IResolverContext context, Guid area_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.AREA WHERE ARE_ID = @area_id";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { area_id });
                return exec > 0;
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }
    }
}