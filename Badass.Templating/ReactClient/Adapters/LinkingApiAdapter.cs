using System.Linq;
using Badass.Model;
using Badass.Templating.Classes;

namespace Badass.Templating.ReactClient.Adapters
{
    public class LinkingApiAdapter : ClientApiAdapter
    {
        private readonly ApplicationType _linkingType;

        public LinkingApiAdapter(ApplicationType type, Domain domain, ApplicationType linkingType) : base(type, domain)
        {
            _linkingType = linkingType;
            LinkingType = new ClassAdapter(linkingType, domain);
        }

        public ClassAdapter LinkingType { get; }

        public string LinkedTypeIdFieldName
        {
            get { return _linkingType.Fields.First(f => f.HasReferenceType && f.ReferencesType == base._type).Name; }
        }

        public override SimpleType SelectAllType { get { return base.SelectAllType; } } // TODO - need to get 'select by' operation that links the two types, and return the result of that
    }
}
