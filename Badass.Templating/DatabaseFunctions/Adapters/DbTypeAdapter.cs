﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Badass.Model;

namespace Badass.Templating.DatabaseFunctions.Adapters
{
    public class DbTypeAdapter
    {
        protected readonly ApplicationType _applicationType;
        private readonly string _operation;
        private readonly Domain _domain;


        public DbTypeAdapter(ApplicationType applicationType, string operation, OperationType operationType, Domain domain)
        {
            _applicationType = applicationType;
            _operation = operation;
            _domain = domain;
            OperationType = operationType;
        }

        public virtual IPseudoField UserIdField
        {
            get
            {
                if (_domain.UserIdentity != null)
                {
                    return new UserIdField(_domain);
                }

                return null;
            }
        }

        public virtual DbTypeAdapter UserType
        {
            get
            {
                if (_domain.UserIdentity != null)
                {
                    return new DbTypeAdapter((ApplicationType)_domain.UserIdentity.Type, null, OperationType.None, _domain);
                }

                return null;
            }
        }

        public List<IPseudoField> NonPrimaryKeyFields
        {
            get
            {
                var list = new List<IPseudoField>();
                list.AddRange(_applicationType.Fields.Where(a => !a.IsIdentity).Select(a => new DbFieldAdapter(a, this)).OrderBy(f => f.Order));
                return list;
            }
        }

        public List<IPseudoField> PrimaryKeyFields
        {
            get
            {
                var list = new List<IPseudoField>();
                list.AddRange(_applicationType.Fields.Where(a => a.IsIdentity).Select(a => new DbFieldAdapter(a, this)).OrderBy(f => f.Order));
                return list;
            }
        }

        public IPseudoField PrimaryKeyField => PrimaryKeyFields.FirstOrDefault();

        public List<IPseudoField> UserEditableFields
        {
            get
            {
                var list = new List<IPseudoField>();
                list.AddRange(_applicationType.Fields.Where(a => a.IsUserEditable()).Select(a => new DbFieldAdapter(a, this)).OrderBy(f => f.Order));
                return list;
            }
        }

        public DbFieldAdapter CreatedByField
        {
            get
            {
                var field = _applicationType.Fields.FirstOrDefault(f => f.IsTrackingUser && f.Name.StartsWith(Field.CreatedFieldName));
                if (field != null)
                {
                    return new DbFieldAdapter(field, this);
                }
                return null;
            }
        }

        public bool HasCreatedByField => CreatedByField != null;

        public DbFieldAdapter ModifiedByField
        {
            get
            {
                var field = _applicationType.Fields.FirstOrDefault(f => f.IsTrackingUser && f.Name.StartsWith(Field.ModifiedFieldName));
                if (field != null)
                {
                    return new DbFieldAdapter(field, this);
                }
                return null;
            }
        }

        public bool HasModifiedByField => ModifiedByField != null;

        public virtual List<IPseudoField> SelectInputFields
        {
            get
            {
                var fields = new List<IPseudoField>();

                if (UserIdField != null)
                {
                    fields.Add(UserIdField);
                }

                return fields;
            }
        }

        public List<IPseudoField> UpdateInputFields
        {
            get
            {
                var fields = PrimaryKeyFields.Union(UserEditableFields.Where(f => f.Edit)).ToList();
                if (UserIdField != null)
                {
                    fields.Add(UserIdField);
                }
                if (ModifiedByField != null)
                {
                    fields.Add(ModifiedByField);                    
                }
                return fields.OrderBy(f => f.Order).ToList();
            }
        }

        public List<IPseudoField> InsertInputFields
        {
            get
            {
                var fields = UserEditableFields.Where(f => f.Add).ToList();
                if (UserIdField != null)
                {
                    fields.Add(UserIdField);
                }
                if (CreatedByField != null)
                {
                    fields.Add(CreatedByField);
                }
                return fields.OrderBy(f => f.Order).ToList();
            }
        }

        public List<IPseudoField> DeleteInputFields
        {
            get
            {
                var fields = PrimaryKeyFields.ToList();
                if (UserIdField != null)
                {
                    fields.Add(UserIdField);
                }
                return fields.OrderBy(f => f.Order).ToList();
            }
        }


        public List<IPseudoField> InsertFields
        {
            get
            {
                var fields  = UserEditableFields.Where(f => f.Add).Union(PrimaryKeyFields).ToList();
                var createdDateTrackingField = _applicationType.Fields.FirstOrDefault(a => a.IsTrackingDate && a.Name == Field.CreatedFieldName);
                if (createdDateTrackingField != null)
                {
                    fields.Add(new DbFieldAdapter(createdDateTrackingField, this));
                }
                var searchField = _applicationType.Fields.FirstOrDefault(f => f.IsSearch);
                if (searchField != null)
                {
                    fields.Add(new DbFieldAdapter(searchField, this));
                }
                if (CreatedByField != null)
                {
                    fields.Add(CreatedByField);
                }
                return fields.OrderBy(f => f.Order).ToList();
            }
        }

        public List<IPseudoField> UpdateFields
        {
            get
            {
                var fields = UserEditableFields.Where(f => f.Edit).ToList();
                var updatedDateTrackingField = _applicationType.Fields.FirstOrDefault(a => a.IsTrackingDate && a.Name == Field.ModifiedFieldName);
                if (updatedDateTrackingField != null)
                {
                    fields.Add(new DbFieldAdapter(updatedDateTrackingField, this));
                }
                if (ModifiedByField != null)
                {
                    fields.Add(ModifiedByField);
                }
                var searchField = _applicationType.Fields.FirstOrDefault(f => f.IsSearch);
                if (searchField != null)
                {
                    fields.Add(new DbFieldAdapter(searchField, this));
                }
                return fields.OrderBy(f => f.Order).ToList();
            }
        }

        public string Name => _applicationType.Name;

        public string Namespace => _applicationType.Namespace;

        public List<DbFieldAdapter> Fields => _applicationType.Fields.Where(f => (!f.IsExcludedFromResults)).Select(a => new DbFieldAdapter(a, this)).ToList();

        public bool HasExcludedFields => _applicationType.Fields.Any(f => f.IsExcludedFromResults);

        public virtual string FunctionName => _applicationType.Name + "_" + _operation;

        public OperationType OperationType { get; }

        public string ShortName => _applicationType.Name[0].ToString().ToLowerInvariant();

        public bool SoftDelete => _applicationType.Fields.Any(a => a.IsDelete);

        public bool HardDelete => _applicationType.DeleteType == DeleteType.Hard;

        public bool NoAddUI => _applicationType.Attributes?.noAddUI == true;

        public bool NoEditUI => _applicationType.Attributes?.noEditUI == true;

        public ApplicationType UnderlyingType => _applicationType;

        public bool AllowAnonView 
        {
            get
            {
                var anon = _applicationType.Attributes?.security?.anon;
                return SecurityUtil.HasViewRights(anon);
            }
        }

        public bool AllowAnonList
        {
            get
            {
                var anon = _applicationType.Attributes?.security?.anon;
                return SecurityUtil.HasListRights(anon);
            }
        }

        public bool AllowAnonAdd
        {
            get
            {
                var anon = _applicationType.Attributes?.security?.anon;
                return SecurityUtil.HasAddRights(anon);
            }
        }

        public bool AllowAnonEdit
        {
            get
            {
                var anon = _applicationType.Attributes?.security?.anon;
                return SecurityUtil.HasEditRights(anon);
            }
        }

        public bool AllowAnonDelete
        {
            get
            {
                var anon = _applicationType.Attributes?.security?.anon;
                return SecurityUtil.HasDeleteRights(anon);
            }
        }

        public bool AllowUserView
        {
            get
            {
                var user = _applicationType.Attributes?.security?.user;
                return user == null || SecurityUtil.HasViewRights(user);
            }
        }

        public bool AllowUserList
        {
            get
            {
                var user = _applicationType.Attributes?.security?.user;
                return user == null || SecurityUtil.HasListRights(user);
            }
        }

        public bool AllowUserReadAll
        {
            get
            {
                var user = _applicationType.Attributes?.security?.user;
                return user != null && !SecurityUtil.Contains(user, SecurityRights.None) && SecurityUtil.Contains(user, SecurityRights.ReadAll);
            }
        }

        public bool AllowUserAdd
        {
            get
            {
                var user = _applicationType.Attributes?.security?.user;
                return user == null || SecurityUtil.HasAddRights(user);
            }
        }

        public bool AllowUserEditAll
        {
            get
            {
                var user = _applicationType.Attributes?.security?.user;
                return user != null && !SecurityUtil.Contains(user, SecurityRights.None) && SecurityUtil.Contains(user, SecurityRights.EditAll);
            }
        }

        public bool AllowUserEdit
        {
            get
            {
                var user = _applicationType.Attributes?.security?.user;
                return user == null || SecurityUtil.HasEditRights(user);
            }
        }

        public bool AllowUserDelete
        {
            get
            {
                var user = _applicationType.Attributes?.security?.user;
                return user == null || SecurityUtil.HasDeleteRights(user);
            }
        }

        protected void GetRelatedOwnershipExpression(string currentUserIdentifier, IEnumerable<Field> relatedIdentity, StringBuilder sb, Func<Field, string> getAliasFunc)
        {
            sb.AppendLine("\t(");
            foreach (var identityField in relatedIdentity)
            {
                
                sb.AppendLine(
                    $"\t\t{getAliasFunc(identityField)}.{Util.EscapeSqlReservedWord(identityField.Name)} = {currentUserIdentifier}");
                if (identityField != relatedIdentity.Last())
                {
                    sb.AppendLine("\t\t OR");
                }
            }
            sb.AppendLine("\t)");
        }

        // find a related type with a created by or modified by field
        public List<Field> LinkToOwershipType
        {
            get
            {
                var fields = new List<Field>();

                // TODO - this should be generalised to do something recursive, to find the path no matter how complicated the graph.

                // first level traversal 
                if (GenerateLinkToOwershipType(fields, _applicationType)) return fields;

                // second-level relationships
                var related = _applicationType.Fields.Where(f => f.HasReferenceType);
                foreach (var field in related)
                {
                    fields = new List<Field>();
                    fields.Add(field);
                    if (GenerateLinkToOwershipType(fields, field.ReferencesType))
                    {
                        return fields;
                    }
                }

                return null; // TODO
            }
        }

        private bool GenerateLinkToOwershipType(List<Field> fields, ApplicationType type)
        {
            var directlink = GetFieldsLinkingToOwnedType(type);
            if (directlink != null)
            {
                fields.Add(directlink);
                fields.Add(directlink.ReferencesType.Fields.SingleOrDefault(f => f.IsTrackingUser));
                return true;
            }

            return false;
        }

        private Field GetFieldsLinkingToOwnedType(ApplicationType type)
        {
            return type.Fields.SingleOrDefault(f => f.HasReferenceType && f.ReferencesType.Fields.Any(fld => fld.IsTrackingUser));
        }

        private List<Field> GetRelatingFields(ApplicationType type)
        {
            return type.Fields.Where(f => f.HasReferenceType).ToList();
        }

        protected void GetDirectOwnershipExpression(string currentUserIdentifier, StringBuilder sb, string alias)
        {
            var aliasExp = alias;
            if (!string.IsNullOrEmpty(aliasExp))
            {
                aliasExp += ".";
            }

            if (HasCreatedByField)
            {
                sb.AppendLine($"{aliasExp}{CreatedByField.Name} = {currentUserIdentifier}");
            }

            if (HasCreatedByField && HasModifiedByField)
            {
                sb.AppendLine("OR");
            }

            if (HasModifiedByField)
            {
                sb.AppendLine($"{aliasExp}{ModifiedByField.Name} = {currentUserIdentifier}");
            }
        }
    }

    public enum OperationType
    {
        Select,
        Insert,
        Update,
        Delete,
        None
    }
}