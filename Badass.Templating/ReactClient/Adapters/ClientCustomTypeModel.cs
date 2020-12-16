using System.Collections.Generic;
using System.Linq;
using Badass.Model;
using Badass.Templating.Classes;

namespace Badass.Templating.ReactClient.Adapters
{
    public class ClientCustomTypeModel
    {
        public ClientCustomTypeModel(OperationAdapter operation)
        {
            Name = operation.Name + "Model";
            Fields = operation.UserProvidedParameters.Select(p => new SimpleField {Name = p.Name, ClrType = p.ClrType}).ToList();
        }

        public ClientCustomTypeModel(ResultType resultType)
        {
            Name = resultType.Name;
            Fields = resultType.Fields.Select(f => new SimpleField() {ClrType = f.ClrType, Name = f.Name}).ToList();
        }
        
        public string Name { get;  }
        
        public List<SimpleField> Fields { get;  }
    }
}