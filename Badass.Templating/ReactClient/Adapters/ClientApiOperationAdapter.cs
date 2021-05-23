﻿using System;
using System.Collections.Generic;
using System.Linq;
using Badass.Model;
using Badass.Templating.Classes;
using Badass.Templating.Classes.Adapters;

namespace Badass.Templating.ReactClient.Adapters
{
    public class ClientApiOperationAdapter : OperationAdapter
    {
        public ClientApiOperationAdapter(Operation op, Domain domain, ApplicationType type) : base(op, domain, type)
        {
        }

        public bool HasCustomType => UsesModel || Parameters.Any(p => p.IsCustomTypeOrCustomArray);
        
        public List<Field> EditableLinkingFields
        {
            get
            {
                var fields = UserEditableParameters
                    .Where(p => p.RelatedTypeField != null && p.RelatedTypeField.HasReferenceType &&
                                !p.RelatedTypeField.ReferencesType.IsReferenceData).Select(p => p.RelatedTypeField).ToList();
                
                fields.AddRange(UserEditableParameters.Where(p => p.IsCustomTypeOrCustomArray).SelectMany(p => p.CustomType.Fields.Where(f => f.HasReferenceType && !f.ReferencesType.IsReferenceData)));

                return fields;
            }
        }

        public string StatePath => UsesModel ? "this.state.data" : "this.state";

        public List<UserInputFieldModel> UserInputFields {
            get
            {
                var fields = new List<UserInputFieldModel>();
                foreach (var parameter in UserEditableParameters)
                {
                    if (parameter.IsCustomTypeOrCustomArray)
                    {
                        foreach (var field in parameter.ClientCustomType.Fields)
                        {
                            // this only handles 1 level of nesting of fields
                            fields.Add(new UserInputFieldModel()
                                {Field = field, Name = field.Name, RelativeStatePath = parameter.Name + "."});
                        }
                    }
                    else
                    {
                        fields.Add(new UserInputFieldModel{Field = parameter.RelatedTypeField, Name = parameter.Name, Parameter = parameter});
                    }
                }

                return fields;
            }
        }

        public List<UserInputFieldModel> ClientSuppliedFields
        {
            get
            {
                var fields = UserInputFields;
                if (_op.ChangesData && !_op.CreatesNew)
                {
                    foreach (var parameter in Parameters)
                    {
                        if (parameter.IsCustomTypeOrCustomArray)
                        {
                            foreach (var field in parameter.CustomType.Fields.Where(f => f.IsIdentity))
                            {
                                // this only handles 1 level of nesting of fields
                                fields.Add(new UserInputFieldModel()
                                    {Field = field, Name = field.Name, RelativeStatePath = parameter.Name + "."});
                            }
                        }
                        else
                        {
                            if (parameter.RelatedTypeField?.IsIdentity == true)
                            {
                                fields.Add(new UserInputFieldModel{Field = parameter.RelatedTypeField, Name = parameter.Name, Parameter = parameter});
                            }
                        }
                    }
                }

                return fields;
            }
        }
    }

    public class UserInputFieldModel
    {
        public Field Field { get; set; }
        
        public Parameter Parameter { get; set; }
        
        public string RelativeStatePath { get; set; }
        
        public string Name { get; set; }

        public string NameWithPath => RelativeStatePath + Name;

        public string NameWithPathCamelCase => Util.CamelCase(RelativeStatePath) + Util.CamelCase(Name);

        public string NameWithPathSafeCamelCase => NameWithPathCamelCase.Replace(".", "?.");
        
        public string NameWithPathSafe => NameWithPath.Replace(".", "?.");

        public bool IsBoolean => Field?.IsBoolean ?? Parameter.IsBoolean;

        public bool IsDate => Field?.IsDate ?? Parameter.IsDate;

        public bool IsDateTime => Field?.IsDateTime ?? Parameter.IsDateTime;

        public bool IsLargeTextContent => Field?.IsLargeTextContent ?? Parameter.IsLargeTextContent;

        public bool IsFile => Field?.IsFile ?? Parameter.IsFile;

        public bool IsRating => Field?.IsRating ?? Parameter.IsRating;

        public bool IsColor => Field?.IsColor ?? false;

        public Type ClrType => Field?.ClrType ?? Parameter.ClrType;
    }
}