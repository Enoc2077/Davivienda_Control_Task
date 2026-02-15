using Dapper;
using Davivienda.GraphQL.DataBases;
using Davivienda.Models.Modelos;
using Davivienda.QueryBuilder.Builder;
using HotChocolate.Language;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Services
{
    public class DocumentacionServices
    {
        private readonly DataBase dataBase;
        private readonly DocumentacionQueryBuilder docBuilder;

        public DocumentacionServices(DataBase dataBase, DocumentacionQueryBuilder builder)
        {
            this.dataBase = dataBase;
            this.docBuilder = builder;
        }

        public async Task<IEnumerable<DocumentacionModel>> GetDocumentaciones(IResolverContext context)
        {
            try
            {
                var selections = context.Selection.SyntaxNode.SelectionSet.Selections;
                var fields = selections.OfType<FieldNode>()
                                       .Where(f => f.Name.Value != "__typename")
                                       .Select(f => $"d.{f.Name.Value.ToUpper()}")
                                       .ToList();

                string selectFields = fields.Any() ? string.Join(", ", fields) : "d.*";
                string sqlQuery = $"SELECT {selectFields} FROM dbo.DOCUMENTACION d";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<DocumentacionModel>(sqlQuery);
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<DocumentacionModel?> GetDocumentacionById(IResolverContext context, Guid doc_id)
        {
            try
            {
                string sqlQuery = "SELECT d.* FROM dbo.DOCUMENTACION d WHERE d.DOC_ID = @doc_id";
                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryFirstOrDefaultAsync<DocumentacionModel>(sqlQuery, new { doc_id });
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> InsertDocumentacion(IResolverContext context, DocumentacionModel doc)
        {
            try
            {
                if (doc.DOC_ID == Guid.Empty) doc.DOC_ID = Guid.NewGuid();
                if (doc.DOC_FEC_CRE == default) doc.DOC_FEC_CRE = DateTimeOffset.Now;

                string sqlQuery = @"INSERT INTO dbo.DOCUMENTACION 
                                    (DOC_ID, DOC_NOM, DOC_RUT, TAR_ID, USU_ID, DOC_FEC_CRE, DOC_FEC_MOD) 
                                    VALUES 
                                    (@DOC_ID, @DOC_NOM, @DOC_RUT, @TAR_ID, @USU_ID, @DOC_FEC_CRE, @DOC_FEC_MOD)";

                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, doc);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<IEnumerable<DocumentacionModel>> GetDocumentacionByName(IResolverContext context, string nombre)
        {
            try
            {
                // Buscamos coincidencias en el nombre del documento (DOC_NOM)
                string sqlQuery = "SELECT d.* FROM dbo.DOCUMENTACION d WHERE d.DOC_NOM LIKE @nombre";

                await dataBase.ConnectAsync();
                return await dataBase.Connection.QueryAsync<DocumentacionModel>(
                    sqlQuery,
                    new { nombre = $"%{nombre}%" }
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetDocumentacionByName: {ex.Message}", ex);
            }
            finally
            {
                await dataBase.DisconnectAsync();
            }
        }

        public async Task<bool> UpdateDocumentacion(IResolverContext context, DocumentacionModel doc)
        {
            try
            {
                await dataBase.ConnectAsync();
                var existing = await dataBase.Connection.QueryFirstOrDefaultAsync<DocumentacionModel>(
                    "SELECT * FROM dbo.DOCUMENTACION WHERE DOC_ID = @DOC_ID", new { doc.DOC_ID });

                if (existing == null) return false;

                string sqlQuery = @"UPDATE dbo.DOCUMENTACION SET 
                                    DOC_NOM = @DOC_NOM, DOC_RUT = @DOC_RUT, 
                                    TAR_ID = @TAR_ID, USU_ID = @USU_ID, 
                                    DOC_FEC_MOD = @DOC_FEC_MOD WHERE DOC_ID = @DOC_ID";

                var parameters = new
                {
                    DOC_ID = doc.DOC_ID,
                    DOC_NOM = !string.IsNullOrEmpty(doc.DOC_NOM) ? doc.DOC_NOM : existing.DOC_NOM,
                    DOC_RUT = !string.IsNullOrEmpty(doc.DOC_RUT) ? doc.DOC_RUT : existing.DOC_RUT,
                    TAR_ID = doc.TAR_ID ?? existing.TAR_ID,
                    USU_ID = doc.USU_ID ?? existing.USU_ID,
                    DOC_FEC_MOD = DateTimeOffset.Now
                };

                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, parameters);
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }

        public async Task<bool> DeleteDocumentacion(IResolverContext context, Guid doc_id)
        {
            try
            {
                string sqlQuery = "DELETE FROM dbo.DOCUMENTACION WHERE DOC_ID = @doc_id";
                await dataBase.ConnectAsync();
                var exec = await dataBase.Connection.ExecuteAsync(sqlQuery, new { doc_id });
                return exec > 0;
            }
            finally { await dataBase.DisconnectAsync(); }
        }
    }
}