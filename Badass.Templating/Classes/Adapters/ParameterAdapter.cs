using System;
using Badass.Model;

namespace Badass.Templating.Classes
{
    public class ParameterAdapter : Parameter
    {
        private readonly Parameter _parameter;

        public ParameterAdapter(Domain domain, Parameter parameter) : base(domain)
        {
            _parameter = parameter;
        }

        public override string Name => _parameter.Name;

        public override int Order => _parameter.Order;

        public override Type ClrType => _parameter.ClrType;

        public override string ProviderTypeName => _parameter.ProviderTypeName;

        public override Field RelatedTypeField => _parameter.RelatedTypeField;

        public bool IsColor => _parameter.RelatedTypeField?.Attributes?.type == Field.ColorFieldType;

        public bool HasSize => _parameter.Size != null;

        public override int? Size => _parameter.Size;

        public override bool IsRequired => _parameter.IsRequired;

        public bool RelatedFieldHasReferenceType => _parameter.RelatedTypeField != null && _parameter.RelatedTypeField.HasReferenceType;

        public bool IsInt => _parameter.ClrType == typeof(int) || _parameter.ClrType == typeof(int?); // needed in react template to determine if we need to parseInt or not

        public bool UserEditable
        {
            get
            {
                return _parameter?.Attributes?.userEditable == true || (_parameter.RelatedTypeField != null &&
                                                                       _parameter.RelatedTypeField.IsUserEditable());
            }
        }

        public string ResolvedClrType
        {
            get
            {
                if (!IsCustomType)
                {
                    return Util.FormatClrType(_parameter.ClrType);
                }

                return Util.CSharpNameFromName(_parameter.ProviderTypeName);
            }
        }

        public string ResolvedTypescriptType
        {
            get
            {
                if (!IsCustomType)
                {
                    return Util.GetTypeScriptTypeForClrType(_parameter.ClrType);
                }

                return Util.CSharpNameFromName(_parameter.ProviderTypeName);
            }
        }

        public bool IsCustomType => ClrType == typeof(ResultType);
    }
}
