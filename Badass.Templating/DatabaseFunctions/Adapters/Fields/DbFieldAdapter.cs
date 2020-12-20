using System;
using System.Linq;
using Badass.Model;

namespace Badass.Templating.DatabaseFunctions.Adapters
{
    public class DbFieldAdapter : IPseudoField
    {
        private readonly Field _field;
        private readonly DbTypeAdapter _parent;

        public DbFieldAdapter(Field field, DbTypeAdapter parent)
        {
            _field = field;
            _parent = parent;
        }

        public string Value
        {
            get
            {
                if (_parent.OperationType == OperationType.Insert)
                {
                    if (_field.IsIdentity && _field.ClrType == typeof(int))
                    {
                        return "DEFAULT";
                    }

                    if (_field.IsIdentity && _field.ClrType == typeof(Guid))
                    {
                        return "new_id";
                    }
                    if (_field.IsTrackingDate)
                    {
                        return "clock_timestamp()";
                    }
                    if (_field.IsSearch)
                    {
                        return GetSearchFieldsAsTsVector();
                    }

                    if (_field.IsTrackingUser)
                    {
                        return _parent.FunctionName + "." + _field.Name;    
                    }

                    if (_parent.UserEditableFields.Count == 1)
                    {
                        return _parent.FunctionName + "." + _field.Name;
                    }

                    if (_parent.AddMany)
                    {
                        return _parent.AddManyArrayItemVariableName + "." + _field.Name;
                    }
                    
                    return _parent.NewRecordParamterName + "." + _field.Name;
                }

                if (_parent.OperationType == OperationType.Update)
                {
                    if (_field.IsIdentity)
                    {
                        return _parent.FunctionName + "." + _field.Name;
                    }
                    if (_field.IsTrackingDate)
                    {
                        return "clock_timestamp()";
                    }
                    if (_field.IsSearch)
                    {
                        return GetSearchFieldsAsTsVector();
                    }
                    return _parent.FunctionName + "." + _field.Name;
                }

                return "FIXME";
            }
        }

        private string GetSearchFieldsAsTsVector()
        {
            var textFields = _parent.UnderlyingType.Fields.Where(f => f.ClrType == typeof(string)).Select(f => _parent.FunctionName + "." + f.Name);
            return "to_tsvector(" + string.Join(" || ' ' || ", textFields) + ")";
        }

        public string Name => _field.Name;
        public string ParentAlias => _parent.ShortName;

        public string ProviderTypeName => _field.ProviderTypeName;
        public bool HasDisplayName => false;
        public string DisplayName => null;

        public DbTypeAdapter Parent => _parent;

        public DbFieldAdapter ReferencesTypeField
        {
            get
            {
                return new DbFieldAdapter(_field.ReferencesTypeField, _parent); // not sure if parent is the right thing to pass here...because this references a different type 
            }
        }

        public int Order => _field.Order;

        public bool IsUuid => _field.ClrType == typeof(Guid);
        public bool Add => _field.Add;
        public bool Edit => _field.Edit;

        public bool IsUserEditable => _field.IsUserEditable();

        public bool HasSize => _field.Size != null;

        public int? Size => _field.Size;
    }
}