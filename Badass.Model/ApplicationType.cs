using System.Collections.Generic;
using System.Linq;

namespace Badass.Model
{
    public class SimpleType
    {
        public const string DefaultNamespace = "public";

        public SimpleType(string name, string ns)
        {
            Name = name;
            Namespace = ns;
            Fields = new List<Field>();
        }

        public string Name { get; }

        public string Namespace { get; }

        public List<Field> Fields { get; }

        // TODO - this assumes the first text field is the best "summary" of a thing
        // which may not be the case. We could use some attributes to control which field(s) make up the 
        // "summary" for a related type
        public Field DisplayField => Fields.OrderBy(f => f.Rank).FirstOrDefault(f => f.ClrType == typeof(string));

        public dynamic Attributes { get; set; }
    }

    public class ResultType : SimpleType
    {
        public ResultType(string name, string ns, ApplicationType relatedType) : base(name, ns)
        {
            Operations = new List<Operation>();
            RelatedType = relatedType;
        }

        public ApplicationType RelatedType { get; }
        
        public List<Operation> Operations { get; }

        public bool Ignore => Operations.All(op => op.Ignore);
    }

    public class ApplicationType : SimpleType
    {
        public ApplicationType(string name, string ns) : base(name, ns)
        {
            Constraints = new List<Constraint>();
        }

        public bool IsReferenceData => Attributes?.type == "reference";

        public bool IsSearchable
        {
            get { return Fields.Any(f => f.IsSearch); }
        }

        public DeleteType DeleteType
        {
            get
            {
                if (Fields.Any(f => f.IsDelete))
                {
                    return DeleteType.Soft;
                }

                if (Attributes?.hardDelete == true)
                {
                    return DeleteType.Hard;
                }

                return DeleteType.None;
            }
        }

        public bool Ignore => Attributes?.ignore == true;

        public List<Constraint> Constraints { get; }

        public bool GenerateUI => GenerateApi && !(Attributes?.ui == false);

        public bool GenerateApi => !Ignore && !(Attributes?.api == false);

        public bool IsLink
        {
            get
            {
                return Fields.Count(f => f.HasReferenceType) > 1 && !Fields.Any(f => f.IsUserEditable() && !f.HasReferenceType);
            }
        }

        public bool IsAttachment
        {
            get
            {
                if (Attributes?.isAttachment != null)
                {
                    return (bool) Attributes?.isAttachment;
                }

                return Name == "attachment" && Fields.Any(f => f.IsFile);
            }
        }

        public bool IsSecurityPrincipal => Attributes?.isSecurityPrincipal == true;

        public int Rank // not as useful as I was hoping it would be
        {
            get { return Fields.Count(f => f.HasReferenceType && !f.ReferencesType.IsReferenceData && !f.ReferencesType.IsSecurityPrincipal && !f.ReferencesType.IsLink && !f.ReferencesType.Ignore && !f.ReferencesType.IsAttachment ); }
        }

        public bool Important => Attributes?.important == true;
    }

    public enum DeleteType
    {
        None,
        Hard,
        Soft
    }
}
