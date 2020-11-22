using System;
using System.Collections.Generic;
using System.Linq;
using Badass.Model;

namespace Badass.Templating.DatabaseFunctions.Adapters
{
    public class SelectForDisplayViaLinkDbTypeAdapter : SelectForDisplayDbTypeAdapter
    {
        private readonly ApplicationType _linkingType;
        private string _linkTypeAlias;

        public SelectForDisplayViaLinkDbTypeAdapter(ApplicationType applicationType, string operation, ApplicationType linkingType, Domain domain) : base(applicationType, operation, domain)
        {
            _linkingType = linkingType ?? throw new ArgumentNullException(nameof(linkingType));
        }

        public ApplicationType LinkingType => _linkingType;

        public string LinkTypeAlias
        {
            get
            {
                if (_linkTypeAlias == null)
                {
                    _linkTypeAlias = CreateAliasForLinkingField(LinkingTypeFieldRaw);
                }

                return _linkTypeAlias;
            }
        }

        public DbFieldAdapter LinkingTypeField // this would need to be a List<DbFieldAdapter> and the template do a foreach for this to handle composite primary keys
        {
            get
            {
                return new DbFieldAdapter(LinkingTypeFieldRaw, this);
            }
        }

        private Field LinkingTypeFieldRaw => _linkingType.Fields.SingleOrDefault(f => f.HasReferenceType && f.ReferencesType == this._applicationType && !f.IsTrackingUser);

        public DbFieldAdapter LinkTypeOtherField
        {
            get
            {
                return _linkingType.Fields.Where(f => f.HasReferenceType && f.ReferencesType != this._applicationType && !f.ReferencesType.IsSecurityPrincipal).Select(f => new DbFieldAdapter(f, this)).SingleOrDefault();
            }
        }

        public override List<IPseudoField> SelectInputFields
        {
            get
            {
                var fields = base.SelectInputFields;
                fields.Add(LinkTypeOtherField);
                return fields;
            }
        }

        public override string FunctionName => _applicationType.Name + "_select_via_" + LinkTypeOtherField.Name;
    }
}
