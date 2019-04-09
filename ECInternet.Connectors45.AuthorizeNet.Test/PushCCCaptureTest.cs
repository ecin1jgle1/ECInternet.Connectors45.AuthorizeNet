using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Realisable.Data.Transform;
using Realisable.JobDefinition;
using Realisable.Resources;
using Realisable.Utils;
using Realisable.Utils.DTO;

using ECInternet.Connectors45.Authorize_Net;
namespace ECInternet.Connectors45.Authorize_Net.Test
{
	[TestClass]
	public class PushCCCaptureTest : ConnectorTest
	{
		public const string ENTITY = "Capture";

		[TestMethod]
		public void CCCaptureTest()
		{
			//string ApiLoginID = "9r95gdR4N7T3";
			//string ApiTransactionKey = "7C2p9DN6K8VztH5R";
			decimal TransactionAmount = 1.00M;
			string TransactionID = "61620710013";

			// Create the TransformDefinition for the Order datastructure.
			TransformDefinition tfmDef = CreateCaptureDefinition();

			// Create a TransactionFactory object.
			var factory = new TransactionFactory(tfmDef);

			// Get a reference to the SalesOrder definition.
			TransformRecordDefinition recDef = tfmDef.RecordDefById(ENTITY);

			// Create the SalesOrder transaction.
			Transaction orderTransaction = TransactionFactory.CreateTransaction(recDef, true);

			// It's not necessary to set every field in a test...just those you want!
			//orderTransaction.SetFieldValue("ApiLoginID", ApiLoginID, true);
			//orderTransaction.SetFieldValue("ApiTransactionKey", ApiTransactionKey, true);
			orderTransaction.SetFieldValue("TransactionAmount", TransactionAmount, true);
			orderTransaction.SetFieldValue("TransactionID", TransactionID, true);

			eERPUpdateOperation updateOperation = eERPUpdateOperation.eUpdate;

			// Insert the transaction into the data structure
			TransactionFactory.InsertTransaction(factory.Data, orderTransaction);
			SystemConnectorDTO systemConnector = new SystemConnectorDTO(TestConstants.CONNECTOR_SYSTEM);

			ProcessTransactions(factory, tfmDef, updateOperation, systemConnector);

			// TODO: Performs some tests to ensure the data being 'Pushed' to the application has been
			// accepted and the fields have been populated with correct values.
		}

		private TransformDefinition CreateCaptureDefinition(eERPUpdateOperation updateOperation = eERPUpdateOperation.eUpdate)
		{
			// Create the resultant object.
			TransformDefinition result = CreatePushStyleTransformDef(TestConstants.CONNECTOR_SYSTEM, ImportTypeEnum.Capture, updateOperation);

			// Create the Record Definition
			TransformRecordDefinition order = TransformDefinitionBuilder.CreateRecord(result, "Capture", string.Empty);

			TransformDefinitionBuilder.CreateField(order, nameof(Capture.ApiLoginID));// "ApiLoginID");
			TransformDefinitionBuilder.CreateField(order, nameof(Capture.ApiTransactionKey));
			TransformDefinitionBuilder.CreateField(order, nameof(Capture.TransactionAmount));
			TransformDefinitionBuilder.CreateField(order, nameof(Capture.TransactionID));
			TransformDefinitionBuilder.CreateField(order, nameof(Capture.ResponseCode));
			TransformDefinitionBuilder.CreateField(order, nameof(Capture.MessageCode));
			TransformDefinitionBuilder.CreateField(order, nameof(Capture.Description));
			TransformDefinitionBuilder.CreateField(order, nameof(Capture.AuthCode));
			TransformDefinitionBuilder.CreateField(order, nameof(Capture.ErrorCode));
			TransformDefinitionBuilder.CreateField(order, nameof(Capture.CaptureErrorMessage));

			return result;
		}

	}

}
