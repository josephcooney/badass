using System;
using System.Collections.Generic;
using System.Linq;
using Badass.Model;
using Badass.Templating.Classes.Adapters;
using Badass.Templating.DatabaseFunctions;

namespace Badass.Templating.Classes
{
    public class ClassAdapter
    {
        protected readonly SimpleType _type;
        protected readonly Domain _domain;

        public ClassAdapter(SimpleType type, Domain domain)
        {
            _type = type;
            _domain = domain;
        }

        public string Name => _type.Name;

        public List<ClassFieldAdapter> Fields => _type.Fields.Select(f => new ClassFieldAdapter(f)).ToList();

        public string Namespace
        {
            get
            {
                if (!string.IsNullOrEmpty(_domain.DefaultNamespace) && (string.IsNullOrEmpty(_type.Namespace) || _type.Namespace == SimpleType.DefaultNamespace))
                {
                    return _domain.DefaultNamespace;
                }

                return _type.Namespace;
            }
        }

        public OperationAdapter InsertOperation
        {
            get { return this.Operations.FirstOrDefault(o => o.Name.EndsWith(DbFunctionGenerator.InsertFunctionName)); }
        }

        public OperationAdapter UpdateOperation
        {
            get { return this.Operations.FirstOrDefault(o => o.Name.EndsWith(DbFunctionGenerator.UpdateFunctionName)); }
        }

        public OperationAdapter DeleteOperation
        {
            get { return this.CanDelete ? this.Operations.FirstOrDefault(o => o.Name.EndsWith(DbFunctionGenerator.DeleteOperationName)) : null; }
        }

        public Field IdentityField
        {
            get
            {
                var fields = _type.Fields.Where(a => a.IsIdentity);
                if (fields.Count() == 1)
                {
                    return fields.First();
                }
                if (fields.Count() > 1)
                {
                    throw new InvalidOperationException($"this can't handle composite keys yet, but type {_type.Name} has multiple identity fields.");
                }
                if (fields.Count() == 0)
                {
                    throw new InvalidOperationException($"Type {_type.Name} doesn't have any primary key fields.");
                }
                return null;
            }
        }

        public bool CanDelete => _type is ApplicationType && ((ApplicationType)_type).DeleteType != DeleteType.None;

        public List<ClassFieldAdapter> UserEditableFields => Fields.Where(f => f.IsCallerProvided).ToList();

        public List<ClassFieldAdapter> UserEditableReferenceFields => UserEditableFields
            .Where(a => a.ReferencesType != null && !a.ReferencesType.IsSearchable && a.ReferencesType.DisplayField != null && !a.ReferencesType.Ignore)
            .ToList();

        public List<ApplicationType> UserEditableReferencedTypes => UserEditableReferenceFields.Select(f => f.ReferencesType).Distinct().ToList();

        public bool CanSearch => _type is ApplicationType && ((ApplicationType) _type).Fields.Any(f => f.IsSearch);

        public bool HasCustomResultType
        {
            get { return _domain.Operations.Any(o => !o.Ignore && o.Returns.SimpleReturnType != null && (o.Returns.SimpleReturnType is ResultType) && !((ResultType)o.Returns.SimpleReturnType).Ignore); }
        }

        public virtual List<OperationAdapter> Operations
        {
            get { return _domain.Operations.Where(o => !o.Ignore && (o.Returns?.SimpleReturnType == _type || o.RelatedType == _type)).Select(o => new OperationAdapter(o, _domain, (ApplicationType)_type)).ToList(); }
        }

        public Field DisplayField => _type.DisplayField;

        public bool IsAttachment => _type is ApplicationType && ((ApplicationType) _type).IsAttachment;

        public bool HasDetails
        {
            get
            {
                var t = _type as ApplicationType;
                if (t == null)
                {
                    return false;
                }

                return !t.IsReferenceData && !t.IsSecurityPrincipal && !t.IsLink && !t.Ignore && !t.IsAttachment;
            }
        }
    }
}
