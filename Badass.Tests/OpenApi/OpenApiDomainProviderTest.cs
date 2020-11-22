using System.IO.Abstractions.TestingHelpers;
using Badass.OpenApi;
using Moq;
using Xunit;

namespace Badass.Tests.OpenApi
{
    public class OpenApiDomainProviderTest
    {
        [Fact]
        public void CanAugmentDomainFromOpenApiDocument()
        {
            var domain = TestUtil.CreateTestDomain(new MockFileSystem());
            var openApiDocProvider = new Mock<IOpenApiDocumentProvider>();
            
            // TODO - set this up to return something
            
            var openApiProvider = new OpenApiDomainProvider(openApiDocProvider.Object);
            openApiProvider.AugmentDomainFromOpenApi(domain);
        }
    }
}