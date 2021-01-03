using FluentAssertions;
using Xunit;

namespace Badass.Tests.Templating.Util
{
    public class PluralTests
    {
        [Fact]
        public void SimpleWordIsPluralised()
        {
            var plural = Badass.Templating.Util.Pluralise("cat");
            plural.Should().Be("cats");
        }
        
        [Fact]
        public void LastWordOfSimplePhraseIsPluralised()
        {
            var plural = Badass.Templating.Util.Pluralise("the cat");
            plural.Should().Be("the cats");
        }
        
        [Fact]
        public void WordEndingInYIsCorrectlyPluralised()
        {
            var plural = Badass.Templating.Util.Pluralise("category");
            plural.Should().Be("categories");
        }
        
        [Fact]
        public void PluralIsRetained()
        {
            var plural = Badass.Templating.Util.Pluralise("categories");
            plural.Should().Be("categories");
        }
    }
}