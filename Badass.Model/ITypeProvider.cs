using System.Text;

namespace Badass.Model
{
    public interface ITypeProvider
    {
        Domain GetDomain(Settings settings);

        void GetOperations(Domain domain);

        void AddGeneratedOperation(string text);

        void DropGeneratedOperations(Settings settings, StringBuilder stringBuilder);
        void DropGeneratedTypes(Settings settings, StringBuilder stringBuilder);

        string EscapeReservedWord(string name);

        public string GetCsDbTypeFromDbType(string dbTypeName);

        public string GetSqlName(string entityName);
    }
}
