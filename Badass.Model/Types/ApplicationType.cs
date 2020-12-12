﻿using System.Collections.Generic;
using System.Linq;

namespace Badass.Model
{
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
