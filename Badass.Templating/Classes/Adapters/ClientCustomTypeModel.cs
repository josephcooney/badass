using System.Collections.Generic;
using System.Linq;
using Badass.Model;
using Badass.Templating.ReactClient.Adapters;

namespace Badass.Templating.Classes.Adapters
{
    public class ClientCustomTypeModel
    {
        public ClientCustomTypeModel(OperationAdapter operation)
        {
            Name = operation.Name + "Model";
            Fields = operation.UserProvidedParameters.Select(p => p.RelatedTypeField).ToList();
            Namespace = operation.Namespace;
        }

        public ClientCustomTypeModel(ResultType resultType)
        {
            Name = resultType.Name;
            Fields = resultType.Fields.ToList();
            Namespace = resultType.Namespace;
        }
        
        public string Name { get;  }
        
        public List<Field> Fields { get;  }
        
        public string Namespace { get;  }
    }
}