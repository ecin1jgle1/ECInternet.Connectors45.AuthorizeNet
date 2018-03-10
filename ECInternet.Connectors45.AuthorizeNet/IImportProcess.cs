using Realisable.Data.Transform;
using Realisable.Resources;
using Realisable.Utils.DTO;

namespace ECInternet.Connectors45.Authorize_Net
{
	interface IImportProcess
	{
		void Initialise(string transformId, TransactionIterator iterator, eERPUpdateOperation updateOperation, eTransformErrorAction errorAction, ImportHelper wrapper, SystemConnectorDTO system, ITransformAuditController auditController);

		void CreateTransactions();
	}
}
