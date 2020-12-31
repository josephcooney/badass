using System.Collections.Generic;
using System.Linq;
using Badass.Model;

namespace Badass.Templating.Classes
{
    public class DomainAdapter
    {
        private readonly Domain _domain;

        public DomainAdapter(Domain domain)
        {
            _domain = domain;
        }

        public List<ApplicationType> IncludedTypes => _domain.Types.Where(a => !a.Ignore).OrderBy(t => t.Name).ToList();

        public List<ResultType> CustomTypes => _domain.ResultTypes.Where(rt => rt.IsCustomType).ToList();

        public string DefaultNamespace => _domain.DefaultNamespace;
    }
}
