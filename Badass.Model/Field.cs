using System;

namespace Badass.Model
{
    public class Field
    {
        private const int RankOffset = 1000000;

        public SimpleType Type { get; }

        public Field(SimpleType type)
        {
            Type = type;
        }

        public const string ModifiedFieldName = "modified";

        public const string CreatedFieldName = "created";

        public const string DeletedFieldName = "deleted";
        
        public const string SoftDeleteFieldName = "deleted_date";

        public const string SearchFieldName = "search_content";

        public const string ContentTypeFieldName = "content_type";

        public const string IdFieldName = "id";

        public const string ThumbnailFieldName = "thumbnail";

        public const string ColorFieldType = "color";

        public const string ThumbnailFieldType = "thumbnail";

        public string Name { get; set; }

        public int Order { get; set; }

        public dynamic Attributes { get; set; }

        public int? Size { get; set; }

        public bool IsRequired { get; set; }

        public ApplicationType ReferencesType { get; set; }
        public Field ReferencesTypeField { get; set; } // if this field is a foreign-key relationship to another type, this is the "column" for the foreign key.

        public bool IsIdentity { get; set; }

        public Type ClrType { get; set; }

        public string ProviderTypeName { get; set; }

        // TODO - convert this to a property
        public bool IsUserEditable()
        {
            return !IsIdentity && !IsTrackingDate && !IsDelete && !IsTrackingUser && !IsSearch && !IsExcludedFromResults;
        }
        
        public bool IsTrackingDate => IsDate && (Name == CreatedFieldName || Name == ModifiedFieldName || Name == SoftDeleteFieldName);

        public bool IsTrackingUser => ((ReferencesType != null && ReferencesType.IsSecurityPrincipal) && (Name.StartsWith(CreatedFieldName) || Name.StartsWith(ModifiedFieldName) || (Type is ApplicationType && ((ApplicationType)Type).DeleteType == DeleteType.Soft && Name.StartsWith(DeletedFieldName))));

        public bool IsDelete => (ClrType == typeof(DateTime) || ClrType == typeof(DateTime?)) && Name == SoftDeleteFieldName;

        public bool IsSearch => ProviderTypeName == "tsvector" && Name == SearchFieldName;

        public bool IsExcludedFromResults => IsDelete || IsSearch;

        public bool HasReferenceType => ReferencesType != null;

        public bool IsDate => (ClrType == typeof(DateTime) || ClrType == typeof(DateTime?));

        public bool IsBoolean => ClrType == typeof(bool) || ClrType == typeof(bool?);

        public bool IsFile => (this.ClrType == typeof(byte[]) || this.ClrType == typeof(byte?[]));

        public bool IsAttachmentContentType
        {
            get
            {
                return Type is ApplicationType && Attributes?.isContentType == true || (((ApplicationType) Type).IsAttachment && ClrType == typeof(string) &&
                       Name == ContentTypeFieldName);
            }
        }

        public bool IsAttachmentThumbnail
        {
            get
            {
                return Type is ApplicationType && (Attributes?.type == ThumbnailFieldType || ((ApplicationType)Type).IsAttachment && IsFile &&
                                                                                             Name == ThumbnailFieldName);
            }
        }

        public bool IsLargeTextContent
        {
            get
            {
                return ClrType == typeof(string) && (Size > 500 || Attributes?.largeContent == true);
            }
        }
        
        public bool IsColor => Attributes?.type == ColorFieldType;

        public int Rank => Attributes?.rank != null ? (int)Attributes?.rank : RankOffset + Order;

        public bool IsRating => ((this.ClrType) == typeof(int) || (this.ClrType) == typeof(short)) &&
                                Attributes?.isRating == true;

        public bool Add => Attributes?.add != null ? Attributes.add : true;

        public bool Edit => Attributes?.edit != null ? Attributes.edit : true;

        public bool IsInt => ClrType == typeof(int);
    }
}