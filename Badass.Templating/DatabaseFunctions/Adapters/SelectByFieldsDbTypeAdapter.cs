using System.Collections.Generic;
using System.Linq;
using Badass.Model;

namespace Badass.Templating.DatabaseFunctions.Adapters
{
    public class SelectByFieldsDbTypeAdapter : DbTypeAdapter
    {
        private readonly bool _isSelectByPrimaryKey;
        private readonly List<Field> _selectFields;

        public SelectByFieldsDbTypeAdapter(ApplicationType applicationType, string operation, List<Field> selectFields, OperationType operationType, Domain domain, bool isSelectByPrimaryKey) : base(applicationType, operation, operationType, domain)
        {
            _isSelectByPrimaryKey = isSelectByPrimaryKey;
            _selectFields = selectFields.Where(f => f != null).ToList();
        }

        public override List<IPseudoField> SelectInputFields
        {
            get
            {
                var fields = new List<IPseudoField>();
                if (SelectFields != null)
                {
                    fields.AddRange(SelectFields);
                }

                if (base.SelectInputFields != null)
                {
                    fields.AddRange(base.SelectInputFields);
                }

                return fields.Where(f => f != null).OrderBy(f => f.Order).ToList();
            }
        }

        public List<DbFieldAdapter> SelectFields
        {
            get
            {
                return _selectFields.Select(a => new DbFieldAdapter(a, this)).ToList();
            }
        }

        public bool ReturnsSingle => _isSelectByPrimaryKey;
    }
}