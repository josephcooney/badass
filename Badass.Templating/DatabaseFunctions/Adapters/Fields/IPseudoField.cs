namespace Badass.Templating.DatabaseFunctions.Adapters
{
    public interface IPseudoField
    {
        string Name { get; }
        string ParentAlias { get; }
        string ProviderTypeName { get; }
        bool HasDisplayName { get; }
        string DisplayName { get; }
        int Order { get; }
        bool IsUuid { get; }
        bool Add { get; }
        bool Edit { get; }
        
        bool IsUserEditable { get; }
    }
}