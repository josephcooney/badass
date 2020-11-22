using NSwag;

namespace Badass.OpenApi
{
    public interface IOpenApiDocumentProvider
    {
        OpenApiDocument GetOpenApiDocument();
    }
}