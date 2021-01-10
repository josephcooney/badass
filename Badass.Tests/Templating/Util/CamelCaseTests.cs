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

        [Fact]
        public void Name_With_Consecutive_Capitals_Is_Camel_Cased_The_Same_As_DotnetCore()
        {
            var cmlCasedName = Badass.Templating.Util.CamelCase("ABC");
            cmlCasedName.Should().Be("abc");
        }
        
        [Fact]
        public void Longer_Name_With_Consecutive_Capitals_Is_Camel_Cased_The_Same_As_DotnetCore()
        {
            var cmlCasedName = Badass.Templating.Util.CamelCase("ABCD");
            cmlCasedName.Should().Be("abcd");
        }
        
        [Fact]
        public void Name_With_Consecutive_Capitals_Later_In_Name_Is_Camel_Cased_The_Same_As_DotnetCore()
        {
            var cmlCasedName = Badass.Templating.Util.CamelCase("AbCDEfGHI");
            cmlCasedName.Should().Be("abCDEfGHI");
        }

        [Fact]
        public void Short_Name_With_Mixed_Case_Is_Capitalized_Correctly()
        {
            var cmlCasedName = Badass.Templating.Util.CamelCase("ABc");
            cmlCasedName.Should().Be("aBc");
        }

        [Fact]
        public void Longer_Name_With_Confusing_Case_Is_Capitalized_Correctly()
        {
            var cmlCasedName = Badass.Templating.Util.CamelCase("ABcDefg");
            cmlCasedName.Should().Be("aBcDefg");
        }
        
        [Fact]
        public void Another_Longer_Name_With_Confusing_Case_Is_Capitalized_Correctly()
        {
            var cmlCasedName = Badass.Templating.Util.CamelCase("ABcDE");
            cmlCasedName.Should().Be("aBcDE");
        }
    }
}
