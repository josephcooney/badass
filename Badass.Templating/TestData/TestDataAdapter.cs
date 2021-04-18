using System;
using System.Collections.Generic;
using System.Linq;
using Badass.Model;
using Bogus;

namespace Badass.Templating.TestData
{
    public class TestDataAdapter
    {
        private readonly ApplicationType _applicationType;

        public TestDataAdapter(ApplicationType applicationType)
        {
            _applicationType = applicationType;
        }

        public List<TestDataField> TestData
        {
            get
            {
                return _applicationType.Fields.Select(f => new TestDataField(f)).ToList();
            }
        }

        public string Name => _applicationType.Name;
    }

    public class TestDataField
    {
        private static Faker faker = new Faker();
        
        public TestDataField(Field f)
        {
            Field = f;
            SetTestValue();
        }
        
        public Field Field { get;  }
        
        public string Value { get; private set; }

        private void SetTestValue()
        {
            if (Field.HasReferenceType)
            {
                // TODO - look up value 
            }
            else
            {
                if (Field.ClrType == typeof(string))
                {
                    if (Field.Size < 10)
                    {
                        Value = Quote(faker.Random.String(Field.Size.Value, Field.Size.Value));
                    }
                    else
                    {
                        if (Field.Size.HasValue)
                        {
                            var size = faker.Random.Number(Field.Size.Value / 2, Field.Size.Value);
                            var value = faker.Random.Words();
                            var sizeToTake = Math.Min(size, value.Length);
                            Value = Quote(value.Substring(0, sizeToTake));
                        }
                        else
                        {
                            Value = Quote(faker.Random.Words());
                        }
                    }
                }
            }
        }

        private string Quote(string value)
        {
            return $"'{value}'";
        }
    }
}