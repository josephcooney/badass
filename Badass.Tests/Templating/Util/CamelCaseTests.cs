using FluentAssertions;
using Xunit;

namespace Badass.Tests.Templating.Util
{
    public class CamelCaseTests
    {
        [Fact]
        public void Name_Without_Underscores_Can_Be_Camel_Cased()
        {
            var cmlCasedName = Badass.Templating.Util.CamelCase("Id");
            cmlCasedName.Should().Be("id");
        }

        [Fact]
        public void LowerCase_Name_Without_Underscores_Can_Be_Camel_Cased()
        {
            var cmlCasedName = Badass.Templating.Util.CamelCase("id");
            cmlCasedName.Should().Be("id");
        }
    }
}
