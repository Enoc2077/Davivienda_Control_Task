using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class ProyectosServices
    {
        private readonly DataBase dataBase;
        private readonly ProyectosQueryBuilder proyBuilder; // Inyección del Builder

        public ProyectosServices(DataBase dataBase, ProyectosQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.proyBuilder = builder; // Asignación del builder
        }

        public async Task<IEnumerable<ProyectosModel>> GetProyectos(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"p.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "p.*";
                // Tabla en singular: dbo.PROYECTO
                string sqlQuery = $"SELECT {selectFields} FROM dbo.PROYECTO p";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<ProyectosModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<ProyectosModel>> GetProyectosByName(IResolverContext context, string nombre)
        {
            try
            {
                // Búsqueda por coincidencia en PRO_NOM sobre la tabla PROYECTO
                string sqlQuery = "SELECT p.* FROM dbo.PROYECTO p WHERE p.PRO_NOM LIKE @nombre";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<ProyectosModel>(
                    sqlQuery,
                    new { nombre = $"%{nombre}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetProyectosByName: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<ProyectosModel?> GetProyectoById(IResolverContext context, Guid pro_id)
        {
            try
            {
                // Tabla en singular: dbo.PROYECTO
                string sqlQuery = "SELECT p.* FROM dbo.PROYECTO p WHERE p.PRO_ID = @pro_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<ProyectosModel>(sqlQuery, new { pro_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> InsertProyecto(IResolverContext context, ProyectosModel proyecto)
        {
            try
            {
                if (proyecto.PRO_ID == Guid.Empty) proyecto.PRO_ID = Guid.NewGuid();
                if (proyecto.PRO_FEC_CRE == default) proyecto.PRO_FEC_CRE = DateTimeOffset.Now;

                // Dapper mapeará solo los campos que coincidan con los @etiquetas
                string sqlQuery = @"INSERT INTO dbo.PROYECTO 
                            (PRO_ID, PRO_NOM, PRO_DES, PRO_FEC_INI, PRO_FEC_FIN, PRO_EST, ARE_ID, PRO_FEC_CRE) 
                            VALUES 
                            (@PRO_ID, @PRO_NOM, @PRO_DES, @PRO_FEC_INI, @PRO_FEC_FIN, @PRO_EST, @ARE_ID, @PRO_FEC_CRE)";

                await dataBase.ConnectAsync();
                // Aunque 'proyecto' tenga la propiedad 'Progreso', Dapper no la usará porque no está en el SQL
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, proyecto);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateProyecto(IResolverContext context, ProyectosModel proyecto)
        {
            try
            {
                await dataBase.ConnectAsync();
                // Tabla en singular: dbo.PROYECTO
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<ProyectosModel>(
                    "SELECT * FROM dbo.PROYECTO WHERE PRO_ID = @PRO_ID", new { proyecto.PRO_ID });

                if (existing == null) return false;

                // Tabla en singular: dbo.PROYECTO
                string sqlQuery = @"UPDATE dbo.PROYECTO SET 
                                    PRO_NOM = @PRO_NOM, PRO_DES = @PRO_DES, PRO_FEC_INI = @PRO_FEC_INI, 
                                    PRO_FEC_FIN = @PRO_FEC_FIN, PRO_EST = @PRO_EST, ARE_ID = @ARE_ID, 
                                    PRO_FEC_MOD = @PRO_FEC_MOD WHERE PRO_ID = @PRO_ID";

                var parameters = new
                {
                    PRO_ID = proyecto.PRO_ID,
                    PRO_NOM = !string.IsNullOrEmpty(proyecto.PRO_NOM) ? proyecto.PRO_NOM : existing.PRO_NOM,
                    PRO_DES = proyecto.PRO_DES ?? existing.PRO_DES,
                    PRO_FEC_INI = proyecto.PRO_FEC_INI == default ? existing.PRO_FEC_INI : proyecto.PRO_FEC_INI,
                    PRO_FEC_FIN = proyecto.PRO_FEC_FIN ?? existing.PRO_FEC_FIN,
                    PRO_EST = proyecto.PRO_EST ?? existing.PRO_EST,
                    ARE_ID = proyecto.ARE_ID ?? existing.ARE_ID,
                    PRO_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteProyecto(IResolverContext context, Guid pro_id)
        {
            try
            {
                // Tabla en singular: dbo.PROYECTO
                string sqlQuery = "DELETE FROM dbo.PROYECTO WHERE PRO_ID = @pro_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { pro_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}