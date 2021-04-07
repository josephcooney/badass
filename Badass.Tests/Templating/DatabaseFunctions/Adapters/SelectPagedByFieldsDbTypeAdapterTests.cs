using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Badass.Model;
using Badass.Templating.DatabaseFunctions.Adapters;
using FluentAssertions;
using Xunit;

namespace Badass.Tests.Templating.DatabaseFunctions.Adapters
{
    public class SelectPagedByFieldsDbTypeAdapterTests
    {
        [Fact]
        public void SelectFieldsAreReturnedWithIndex()
        {
            var domain = TestUtil.CreateTestDomain(new MockFileSystem());
            var orderType = domain.Types.Single(t => t.Name == "order");
            var adapter = new SelectPagedByFieldsDbTypeAdapter(orderType, "test", new List<Field>(orderType.Fields),
                OperationType.Select, domain);

            adapter.SelectFieldsWithIndices.Count.Should().BeGreaterThan(1);
            
            for (var i = 0; i < adapter.SelectFieldsWithIndices.Count; i++)
            {
                adapter.SelectFieldsWithIndices[i].Index.Should().Be(i + 1);    
            }
        }
    }
}