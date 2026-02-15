using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Query
{
    [ExtendObjectType("Query")]
    public class DocumentacionQuery
    {
        public async Task<IEnumerable<DocumentacionModel>> GetDocumentaciones(
            [Service] DocumentacionServices docServices,
            IResolverContext context)
        {
            return await docServices.GetDocumentaciones(context);
        }

        public async Task<DocumentacionModel?> GetDocumentacionById(
            Guid doc_id,
            [Service] DocumentacionServices docServices,
            IResolverContext context)
        {
            return await docServices.GetDocumentacionById(context, doc_id);
        }
    }
}