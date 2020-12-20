using System;
using FluentAssertions;
using Xunit;

namespace Badass.Tests.Templating.Util
{
    public class FormatClrTypeNameTests
    {
        [Fact]
        public void CanGetFormattedClrTypeNameOfCommonTypes()
        {
            Badass.Templating.Util.FormatClrType(typeof(int)).Should().Be("int");
            Badass.Templating.Util.FormatClrType(typeof(int?)).Should().Be("int?");
            Badass.Templating.Util.FormatClrType(typeof(string)).Should().Be("string");
            Badass.Templating.Util.FormatClrType(typeof(Guid)).Should().Be("Guid");
            Badass.Templating.Util.FormatClrType(typeof(Badass.Templating.Util)).Should().Be("Util");
        }
    }
}