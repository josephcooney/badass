using System.Collections.Generic;
using System.Linq;

namespace Badass.Model
{
    public class Domain
    {
        public ITypeProvider TypeProvider { get; }
        
        public Settings Settings { get; }

        public Domain(Settings settings, ITypeProvider typeProvider)
        {
            Settings = settings;
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
                if (Settings.TypeName != null)
                {
                    return Types.Where(t => t.Name == Settings.TypeName).ToList();
                }

                return Types;
            }
        }

        public List<string> ExcludedSchemas => Settings.ExcludedSchemas;

        public ApplicationType UserType => Types.SingleOrDefault(t => t.IsSecurityPrincipal);
        
        public Field UserIdentity
        {
            get
            {
                if (UserType != null)
                {
                    var id = UserType.Fields.Single(f => f.IsIdentity);
                    return id;
                }

                return null;
            }
        }
    }
}
