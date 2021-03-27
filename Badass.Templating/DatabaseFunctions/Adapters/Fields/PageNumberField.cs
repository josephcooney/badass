namespace Badass.Templating.DatabaseFunctions.Adapters.Fields
{
    public class PageNumberField : IPseudoField
    {
        public string Name => "page_num";
        public string ParentAlias => null;
        public string ProviderTypeName => "integer";
        public bool HasDisplayName => false;
        public string DisplayName => null;
        public int Order => 0;
        public bool IsUuid => false;
        public bool Add => false;
        public bool Edit => false;
        public bool IsUserEditable => true;
        public bool IsIdentity => false;
        public bool IsInt => true;
        public bool HasSize => false;
        public int? Size => null;
    }
}