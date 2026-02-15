using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class ComentariosServices
    {
        private readonly DataBase dataBase;
        private readonly ComentariosQueryBuilder comBuilder;

        public ComentariosServices(DataBase dataBase, ComentariosQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.comBuilder = builder;
        }

        // --- QUERIES ---

        public async Task<IEnumerable<ComentariosModel>> GetComentarios(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<HotChocolate.Language.FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"c.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "c.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.COMENTARIOS c";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<ComentariosModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<ComentariosModel>> GetComentariosByText(IResolverContext context, string texto)
        {
            try
            {
                // Buscamos coincidencias dentro del contenido del comentario
                string sqlQuery = "SELECT c.* FROM dbo.COMENTARIOS c WHERE c.COM_COM LIKE @texto";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<ComentariosModel>(
                    sqlQuery,
                    new { texto = $"%{texto}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetComentariosByText: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<ComentariosModel?> GetComentarioById(IResolverContext context, Guid com_id)
        {
            try
            {
                string sqlQuery = "SELECT c.* FROM dbo.COMENTARIOS c WHERE c.COM_ID = @com_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<ComentariosModel>(sqlQuery, new { com_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        // --- MUTATIONS ---

        public async Task<bool> InsertComentario(IResolverContext context, ComentariosModel comentario)
        {
            try
            {
                if (comentario.COM_ID == Guid.Empty) comentario.COM_ID = Guid.NewGuid();
                if (comentario.COM_FEC_CRE == default) comentario.COM_FEC_CRE = DateTimeOffset.Now;

                string sqlQuery = @"INSERT INTO dbo.COMENTARIOS 
                                    (COM_ID, COM_COM, FRI_ID, USU_ID, COM_FEC_CRE, COM_FEC_MOD) 
                                    VALUES 
                                    (@COM_ID, @COM_COM, @FRI_ID, @USU_ID, @COM_FEC_CRE, @COM_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, comentario);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> UpdateComentario(IResolverContext context, ComentariosModel comentario)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<ComentariosModel>(
                    "SELECT * FROM dbo.COMENTARIOS WHERE COM_ID = @COM_ID", new { comentario.COM_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.COMENTARIOS SET 
                                    COM_COM = @COM_COM, FRI_ID = @FRI_ID, USU_ID = @USU_ID, 
                                    COM_FEC_MOD = @COM_FEC_MOD WHERE COM_ID = @COM_ID";

                var parameters = new
                {
                    COM_ID = comentario.COM_ID,
                    COM_COM = !string.IsNullOrEmpty(comentario.COM_COM) ? comentario.COM_COM : existing.COM_COM,
                    FRI_ID = comentario.FRI_ID ?? existing.FRI_ID,
                    USU_ID = comentario.USU_ID ?? existing.USU_ID,
                    COM_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteComentario(IResolverContext context, Guid com_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.COMENTARIOS WHERE COM_ID = @com_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { com_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}