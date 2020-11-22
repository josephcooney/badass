using System.Collections.Generic;
using System.Linq;
using Badass.Model;

namespace Badass.Templating.DatabaseFunctions.Adapters
{
    public class SelectByFieldsForDisplayDbTypeAdapter : SelectForDisplayDbTypeAdapter
    {
        private readonly List<Field> _selectFields;

        public SelectByFieldsForDisplayDbTypeAdapter(ApplicationType applicationType, string operation, List<Field> selectFields, Domain domain) : base(applicationType, operation, domain)
        {
            _selectFields = selectFields;
        }

        public override List<IPseudoField> SelectInputFields
        {
            get
            {
                var fields = new List<IPseudoField>();
                fields.AddRange(this.SelectFields);
                if (UserIdField != null)
                {
                    fields.Add(UserIdField);
                }
                return fields.OrderBy(f => f.Order).ToList();
            }
        }

        public List<DbFieldAdapter> SelectFields
        {
            get
            {
                return _selectFields.Select(a => new DbFieldAdapter(a, this)).ToList();
            }
        }

        public override bool FilterListOperation
        {
            get
            {
                if (IsPrimaryKey)
                {
                    // it's not really a "list all" operation if it is a lookup by primary key
                    return false;
                }

                return base.FilterListOperation;
            }
        }

        public bool IsPrimaryKey => _selectFields.All(f => f.IsIdentity);
    }
}
