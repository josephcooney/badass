using Xunit;

namespace Badass.Tests.Templating.Util
{
    public class CamelCaseTests
    {
        [Fact]
        public void Name_Without_Underscores_Can_Be_Camel_Cased()
        {
            var cmlCasedName = Badass.Templating.Util.CamelCase("Id");
            Assert.Equal("id", cmlCasedName);
        }

        [Fact]
        public void LowerCase_Name_Without_Underscores_Can_Be_Camel_Cased()
        {
            var cmlCasedName = Badass.Templating.Util.CamelCase("id");
            Assert.Equal("id", cmlCasedName);
        }
    }
}
