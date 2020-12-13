﻿using Badass.Model;
using Badass.Templating.DatabaseFunctions.Adapters;
using FluentAssertions;
using Xunit;

namespace Badass.Tests.DbGenerationTests.Aliasing
{
    public class When_Type_Needs_An_Alias
    {
        private string alias = null;

        public When_Type_Needs_An_Alias()
        {
            var type = new ApplicationType("foobar", "test");
            var field = new Field(type){Name = "baz"};
            var aliases = new FieldEntityAliasDictionary();
            alias = aliases.CreateAliasForTypeByField(field);
        }

        [Fact]
        public void An_Alias_Is_Created()
        {
            alias.Should().Be("f");
        }
    }
}
