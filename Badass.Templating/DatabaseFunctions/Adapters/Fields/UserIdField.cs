using Badass.Model;

namespace Badass.Templating.DatabaseFunctions.Adapters
{
    public class UserIdField : IPseudoField
    {
        public UserIdField(Domain domain)
        {
            Name = Parameter.SecurityUserIdParamName;
            ProviderTypeName = domain.UserIdentity.ProviderTypeName;
            HasDisplayName = false;
            Order = 0;
        }

        public string Name { get; }
        public string ParentAlias { get; }
        public string ProviderTypeName { get; }
        public bool HasDisplayName { get; }
        public string DisplayName { get; }
        public int Order { get; }
        public bool IsUuid => false;
        public bool Add => true;
        public bool Edit => true;
    }
}