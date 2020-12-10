using System.Collections.Generic;
using System.Linq;
using Badass.Model;

namespace Badass.Templating.Classes.Repository
{
    public class RepositoryAdapter
    {
        private readonly Domain _domain;

        public RepositoryAdapter(Domain domain, ApplicationType type)
        {
            _domain = domain;
            Type = type;
        }

        public ApplicationType Type { get; }

        public List<DbOperationAdapter> Operations
        {
            get { return _domain.Operations.Where(o => !o.Ignore && o.Attributes?.applicationtype == Type.Name || o.Returns.SimpleReturnType == Type).Select(o => new DbOperationAdapter(o, _domain, Type)).ToList(); }
        }

        public bool HasCustomResultType
        {
            get { return _domain.Operations.Any(o => !o.Ignore && o.Returns.SimpleReturnType != null && (o.Returns.SimpleReturnType is ResultType) && !((ResultType)o.Returns.SimpleReturnType).Ignore); }
        }

        public string Namespace
        {
            get
            {
                if (!string.IsNullOrEmpty(_domain.DefaultNamespace) && (string.IsNullOrEmpty(Type.Namespace) || Type.Namespace == SimpleType.DefaultNamespace))
                {
                    return _domain.DefaultNamespace;
                }

                return Type.Namespace;
            }
        }
    }
}
