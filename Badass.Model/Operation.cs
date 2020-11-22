using System.Collections.Generic;
using System.Linq;

namespace Badass.Model
{
    public class Operation
    {
        public Operation()
        {
            Parameters = new List<Parameter>();
        }

        public string Namespace { get; set; }

        public string Name { get; set; }

        public dynamic Attributes { get; set; }

        public List<Parameter> Parameters { get; }

        public List<Parameter> UserProvidedParameters
        {
            get
            {
                return Parameters.Where(p =>
                    p.RelatedTypeField?.IsTrackingUser != true &&
                    p.RelatedTypeField?.IsAttachmentContentType != true &&
                    p.RelatedTypeField?.IsAttachmentThumbnail != true &&
                    !p.IsSecurityUser).ToList();
            }
        }

        public OperationReturn Returns { get; set; }

        public bool Ignore => Attributes?.ignore == true;
        public bool IsGenerated => Attributes?.generated == true;
        public bool ChangesData => Attributes?.changesData == true;
        public bool CreatesNew => Attributes?.createsNew == true;

        public ApplicationType RelatedType { get; set; } // this is set from the Attributes.applicationType - looked up by name

        public string BareName
        {
            get
            {
                if (RelatedType != null)
                {
                    return Name.Replace(RelatedType.Name, "").Trim('_').Replace("__", "_");
                }

                return null;
            }
        } 
        
        public string FriendlyName
        {
            get
            {
                var friendly = Attributes?.friendlyName;
                if (friendly == null)
                {
                    return BareName;
                }

                return friendly.ToString();
            }
        }
        
        public string CustomReturnTypeName => Attributes?.returnTypeName?.ToString();

        public bool GenerateUI => !Ignore && !(Attributes?.ui == false);

        public bool GenerateApi => !Ignore && !(Attributes?.api == false);

        public bool IsSelectById
        {
            get
            {
                return ReturnsRelatedType && Parameters.Count == 2 && Parameters.All(p =>
                    p.IsSecurityUser || (p.RelatedTypeField != null && p.RelatedTypeField.IsIdentity));
            }
        }

        private bool ReturnsRelatedType
        {
            get
            {
                return (Returns.ReturnType == ReturnType.ApplicationType && Returns.SimpleReturnType == RelatedType) ||
                       Returns.ReturnType == ReturnType.CustomType;
            }
        }

        public bool SingleResult => Attributes?.single_result == true || Returns.ReturnType == ReturnType.Singular;
        
        
    }

    public class OperationReturn
    {
        public ReturnType ReturnType { get; set; }

        public SimpleType SimpleReturnType { get; set; }

        public System.Type SingularReturnType { get; set; }
    }

    public enum ReturnType
    {
        None,
        Singular,
        ApplicationType,
        CustomType
    }
}
