using System.Collections.Generic;
using System.Linq;

namespace Badass.Model
{
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
}