using System.Collections.Generic;
using Badass.Model;
using Badass.Templating.DatabaseFunctions.Adapters.Fields;

namespace Badass.Templating.DatabaseFunctions.Adapters
{
    public class SelectPagedForDisplayViaLinkDbTypeAdapter : SelectForDisplayViaLinkDbTypeAdapter
    {
        public SelectPagedForDisplayViaLinkDbTypeAdapter(ApplicationType applicationType, string operation, ApplicationType linkingType, Domain domain) : base(applicationType, operation, linkingType, domain)
        {
        }
        
        public override List<IPseudoField> SelectInputFields
        {
            get
            {
                var fields = base.SelectInputFields;
                fields.Add(PageSizeField);
                fields.Add(PageNumberField);
                fields.Add(SortField);
                fields.Add(SortDescendingField);
                return fields;
            }
        }

        public IPseudoField PageSizeField => new PageSizeField();

        public IPseudoField PageNumberField => new PageNumberField();

        public IPseudoField SortField => new SortField();

        public IPseudoField SortDescendingField => new SortDescendingField();
    }
}