using System;
using System.Collections.Generic;
using System.Linq;
using Badass.Model;
using Badass.Templating.Classes;

namespace Badass.Templating.ReactClient.Adapters
{
    public class ClientApiOperationAdapter : OperationAdapter
    {
        public ClientApiOperationAdapter(Operation op, Domain domain, ApplicationType type) : base(op, domain, type)
        {
        }

        public bool HasCustomType => UsesModel || Parameters.Any(p => p.IsCustomType);

        public ClientCustomTypeModel CustomType
        {
            get
            {
                if (UsesModel)
                {
                    return new ClientCustomTypeModel(this);
                }

                // this doesn't support multiple custom result types as parameters
                var customParam = Parameters.Single(p => p.IsCustomType);
                return new ClientCustomTypeModel(customParam.CustomType);
            }
        }

        public bool UsesCustomType
        {
            get
            {
                return UsesModel || Parameters.Any(p => p.IsCustomType);
            }
        }

        public List<Field> EditableLinkingFields
        {
            get
            {
                var fields = UserEditableParameters
                    .Where(p => p.RelatedTypeField != null && p.RelatedTypeField.HasReferenceType &&
                                !p.RelatedTypeField.ReferencesType.IsReferenceData).Select(p => p.RelatedTypeField).ToList();
                
                fields.AddRange(UserEditableParameters.Where(p => p.IsCustomType).SelectMany(p => p.CustomType.Fields.Where(f => f.HasReferenceType && !f.ReferencesType.IsReferenceData)));

                return fields;
            }
        }

        public string StatePath => UsesModel ? "this.state.data" : "this.state";
    }

    public class ClientCustomTypeModel
    {
        public ClientCustomTypeModel(OperationAdapter operation)
        {
            Name = operation.Name + "Model";
            Fields = operation.UserProvidedParameters.Select(p => new SimpleField {Name = p.Name, ClrType = p.ClrType}).ToList();
        }

        public ClientCustomTypeModel(ResultType resultType)
        {
            Name = resultType.Name;
            Fields = resultType.Fields.Select(f => new SimpleField() {ClrType = f.ClrType, Name = f.Name}).ToList();
        }
        
        public string Name { get;  }
        
        public List<SimpleField> Fields { get;  }
    }

    public class SimpleField
    {
        public string Name { get; set; }
        
        public Type ClrType { get; set; }
    } 
}