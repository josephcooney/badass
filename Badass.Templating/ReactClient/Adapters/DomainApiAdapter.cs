using System.Collections.Generic;
using System.Linq;
using Badass.Model;

namespace Badass.Templating.ReactClient.Adapters
{
    public class DomainApiAdapter
    {
        private readonly Domain _domain;

        public DomainApiAdapter(Domain domain)
        {
            _domain = domain;
        }

        public List<ApplicationType> TypesThatHaveUI
        {
            get { return _domain.Types.Where(t => t.GenerateUI).OrderBy(t => t.Name).ToList(); }           
        }

        public List<ClientApiAdapter> RelevantTypesToUser
        {
            get
            {
                // owned by the user, and without any relations to anything else - 'root' objects in the graph
                return _domain.Types.Where(t => !t.Ignore && t.GenerateUI && t.Fields.Any(f => f.HasReferenceType && f.ReferencesType.IsSecurityPrincipal) && !t.Fields.Any(f => f.HasReferenceType && !f.ReferencesType.IsSecurityPrincipal) || t.Important).OrderBy(t => t.Name).Select(t => new ClientApiAdapter(t, _domain)).ToList();
            }
        }
    }
}
