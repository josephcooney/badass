using Badass.Model;
using Badass.Templating.Classes;

namespace Badass.Templating.ReactClient.Adapters
{
    public class ClientApiInsertUpdateAdapter : ClientDetailAdapter
    {
        private readonly OperationAdapter _operation;

        public ClientApiInsertUpdateAdapter(SimpleType type, Domain domain, OperationAdapter operation) : base(type, domain)
        {
            _operation = operation;
        }

        public OperationAdapter CurrentOperation => _operation;

        public string OperationName => Util.CSharpNameFromName(_operation.BareName);

        public string OperationNameFriendly => _operation.FriendlyName;

        public bool IsUpdate => !_operation.CreatesNew;

        public bool AssociateViaLink => _operation.CreatesNew && !((ApplicationType) base._type).IsLink;

    }
}
