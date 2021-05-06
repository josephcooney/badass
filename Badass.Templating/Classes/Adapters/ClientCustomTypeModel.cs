using System.Collections.Generic;
using System.Linq;
using Badass.Model;

namespace Badass.Templating.Classes.Adapters
{
    public class ClientCustomTypeModel
    {
        private Domain _domain;
        private string _namespace;
        
        public ClientCustomTypeModel(OperationAdapter operation, Domain domain)
        {
            Name = operation.Name + "Model";
            Fields = operation.UserProvidedParameters.Select(p => p.RelatedTypeField).ToList();
            _domain = domain;
            _namespace = operation.Namespace;
        }

        public ClientCustomTypeModel(ResultType resultType)
        {
            Name = resultType.Name;
            Fields = resultType.Fields.ToList();
            _domain = resultType.Domain;
            _namespace = resultType.Namespace;
        }
        
        public string Name { get;  }
        
        public List<Field> Fields { get;  }
        
        public string Namespace
        {
            get
            {
                if (!string.IsNullOrEmpty(_domain.DefaultNamespace) && (string.IsNullOrEmpty(_namespace) || _namespace == SimpleType.DefaultNamespace))
                {
                    return _domain.DefaultNamespace;
                }

                return _namespace;
            }
        }
    }
}