namespace Badass.Model
{
    public class SecurityRoles
    {
        public const string Anonymous = "anon";
        public const string User = "user";
        public const string Admin = "admin";
    }

    public class SecurityRights
    {
        public const string Add = "add";
        public const string Edit = "edit";
        public const string Delete = "delete";
        public const string Read = "read";
        public const string ReadAll = "read-all";
        public const string EditAll = "edit-all";
        public const string List = "list";
        public const string None = "none";
    }
}
