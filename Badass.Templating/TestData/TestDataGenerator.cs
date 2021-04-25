using System;
using System.Collections.Generic;
using System.Linq;
using Badass.Model;

namespace Badass.Templating.TestData
{
    public class TestDataGenerator : GeneratorBase
    {
        // TODO - use bogus https://github.com/bchavez/Bogus to create better test data
        
        public override List<CodeFile> Generate(Domain domain)
        {
            var files = new List<CodeFile>();
            
            var orderedTables = domain.Types.Where(t => !t.Ignore).OrderBy(t => t.Fields.Count(f => f.ReferencesType != null));
            foreach (var applicationType in orderedTables)
            {
                var file = GenerateTestData(applicationType);
                files.Add(file); 
            }

            return files;
        }

        private CodeFile GenerateTestData(ApplicationType applicationType)
        {
            var adapter = new TestDataAdapter(applicationType);
            return new CodeFile
            {
                Name = applicationType.Name + "_testdata.sql",
                Contents = GenerateFromTemplate(adapter, "TestData"),
            };
        }
    }
}