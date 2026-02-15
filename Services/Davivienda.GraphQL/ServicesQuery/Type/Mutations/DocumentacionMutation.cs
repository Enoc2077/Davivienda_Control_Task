using Davivienda.GraphQL.ServicesQuery.Services;
using Davivienda.Models.Modelos;
using HotChocolate.Resolvers;

namespace Davivienda.GraphQL.ServicesQuery.Type.Mutation
{
    [ExtendObjectType("Mutation")]
    public class DocumentacionMutation
    {
        public async Task<bool> InsertDocumentacion(
            DocumentacionModel doc,
            [Service] DocumentacionServices docServices,
            IResolverContext context)
        {
            return await docServices.InsertDocumentacion(context, doc);
        }

        public async Task<bool> UpdateDocumentacion(
            DocumentacionModel doc,
            [Service] DocumentacionServices docServices,
            IResolverContext context)
        {
            return await docServices.UpdateDocumentacion(context, doc);
        }

        public async Task<bool> DeleteDocumentacion(
            Guid doc_id,
            [Service] DocumentacionServices docServices,
            IResolverContext context)
        {
            return await docServices.DeleteDocumentacion(context, doc_id);
        }
    }
}