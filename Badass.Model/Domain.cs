using System.Collections.Generic;
using System.Linq;

namespace Badass.Model
{
    public class Domain
    {
        public ITypeProvider TypeProvider { get; }
        
        private readonly Settings _settings;

        public Domain(Settings settings, ITypeProvider typeProvider)
        {
            _settings = settings;
            Types = new List<ApplicationType>();
            Operations = new List<Operation>();
            ResultTypes = new List<ResultType>();
            TypeProvider = typeProvider;
        }

        public List<ApplicationType> Types { get;  }

        public List<Operation> Operations { get; }

        public List<ResultType> ResultTypes { get; }

        public string DefaultNamespace { get; set; }

        public List<ApplicationType> FilteredTypes
        {
            get
            {
                if (_settings.TypeName != null)
                {
                    return Types.Where(t => t.Name == _settings.TypeName).ToList();
                }

                return Types;
            }
        }

        public List<string> ExcludedSchemas => _settings.ExcludedSchemas;

        public Field UserIdentity
        {
            get
            {
                var userType = Types.SingleOrDefault(t => t.IsSecurityPrincipal);
                if (userType != null)
                {
                    var id = userType.Fields.Single(f => f.IsIdentity);
                    return id;
                }

                return null;
            }
        }
    }
}
