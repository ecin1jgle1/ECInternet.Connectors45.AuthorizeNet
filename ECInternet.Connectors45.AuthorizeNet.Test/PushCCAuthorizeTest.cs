using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Realisable.Data.Transform;
using Realisable.JobDefinition;
using Realisable.Resources;


namespace ECInternet.Connectors45.Authorize_Net.Test
{
	[TestClass]
	public class PushCCAuthorizeTest : ConnectorTest
	{
		/// <summary>
		/// Gets or sets the test context which provides information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }

		public ImportTypeEnum ImportType()
		{
			return ImportTypeEnum.Authorize;
		}

		/// <summary>
		/// This test illustrates how to create a test for a flat dataset for a 'Push' style connector.
		/// </summary>
		[TestMethod]
		public void FlatDatasetPushConnectorTest()
		{
			// Create the TransformDefinition for the Customer import.
			TransformDefinition tfmDef = CreateAuthorizeDefinition();

			// Create the TransactionFactory to contain the dataset.
			var factory = new TransactionFactory(tfmDef);

			// Get a reference to the Customer definition.
			TransformRecordDefinition recDef = tfmDef.RecordDefById("Customer");

			// Create a Customer transaction object 
			Transaction transaction = TransactionFactory.CreateTransaction(recDef, true);

			// Set the various fields of the Customer transaction

			//transaction.SetFieldValue("Reference", "00090014", false);
			transaction.SetFieldValue("Name", "John Smith", true);
			transaction.SetFieldValue("ShortName", "JohnS", true);
			transaction.SetFieldValue("CreditLimit", 5000M, true);
			transaction.SetFieldValue("Currency", "", true);
			transaction.SetFieldValue("PaymentTermDays", 2, true);
			transaction.SetFieldValue("CountryCode", "UK", true);
			transaction.SetFieldValue("AddressLine1", "91 Brick Lane", true);
			transaction.SetFieldValue("AddressLine2", "", true);
			transaction.SetFieldValue("AddressLine3", "", true);
			transaction.SetFieldValue("AddressLine4", "", true);
			transaction.SetFieldValue("City", "LONDON", true);
			transaction.SetFieldValue("County", "LOONDON", true);
			transaction.SetFieldValue("PostCode", "E1 6QL", true);
			transaction.SetFieldValue("AddressCountryCode", "UK", true);
			transaction.SetFieldValue("MainTelephoneCountryCode", "44", true);
			transaction.SetFieldValue("MainTelephoneAreaCode", "208", true);
			transaction.SetFieldValue("MainTelephoneSubscriberNumber", "1231017", true);
			transaction.SetFieldValue("MainFaxCountryCode", "", true);
			transaction.SetFieldValue("MainFaxAreaCode", "", true);
			transaction.SetFieldValue("MainFaxSubscriberNumber", "", true);
			transaction.SetFieldValue("MainWebSite", "www.realisable.co.uk", true);
			transaction.SetFieldValue("DefaultTaxCode", "1", true);
			transaction.SetFieldValue("TaxRegistrationCode", "GB998728391", true);
			transaction.SetFieldValue("MonthsToKeepTransactions", 1, true);
			transaction.SetFieldValue("OrderPriority", "A", true);
			transaction.SetFieldValue("DUNSCode", "", true);
			transaction.SetFieldValue("UseTaxCodeAsDefault", true, true);
			transaction.SetFieldValue("DefaultNominalCode", "1000", true);
			transaction.SetFieldValue("TradingAccountType", "Trade", true);
			transaction.SetFieldValue("EarlySettlementDiscountPercent", 1.2M, true);
			transaction.SetFieldValue("EarlySettlementDiscountDays", 2, true);
			transaction.SetFieldValue("PaymentTermsDays", 2, true);
			transaction.SetFieldValue("PaymentTermsBasis", 2, true);
			transaction.SetFieldValue("SAGE200IMPSUCCESS", "", true);

			// Insert the Customer transaction into the data structure.
			TransactionFactory.InsertTransaction(factory.Data, transaction);

			ProcessTransactions(factory);

			// TODO: Performs some tests to ensure the data being 'Pushed' to the application has been
			// accepted and the fields have been populated with correct values.
		}

		/// <summary>
		/// Creates the TransformDefinition for the Customer push style connector.
		/// </summary>
		/// <returns></returns>
		private TransformDefinition CreateAuthorizeDefinition(eERPUpdateOperation updateOp = eERPUpdateOperation.eUpdateInsert)
		{
			// Create the resultant object.
			TransformDefinition tfmDef = CreatePushStyleTransformDef(TestConstants.CONNECTOR_SYSTEM, ImportTypeEnum.Authorize, updateOp);

			// Create the Customer RecordDefinition.
			TransformRecordDefinition recDef = TransformDefinitionBuilder.CreateRecord(tfmDef, "Authorize", string.Empty);

			// Add the fields to the Customer Transform.
			TransformDefinitionBuilder.CreateField(recDef, "Reference");
			TransformDefinitionBuilder.CreateField(recDef, "Name");
			TransformDefinitionBuilder.CreateField(recDef, "ShortName");
			TransformDefinitionBuilder.CreateField(recDef, "CreditLimit", IntManFieldTypeEnum.eDecimal);
			TransformDefinitionBuilder.CreateField(recDef, "Currency");
			TransformDefinitionBuilder.CreateField(recDef, "PaymentTermDays");
			TransformDefinitionBuilder.CreateField(recDef, "CountryCode");
			TransformDefinitionBuilder.CreateField(recDef, "AddressLine1");
			TransformDefinitionBuilder.CreateField(recDef, "AddressLine2");
			TransformDefinitionBuilder.CreateField(recDef, "AddressLine3");
			TransformDefinitionBuilder.CreateField(recDef, "AddressLine4");
			TransformDefinitionBuilder.CreateField(recDef, "City");
			TransformDefinitionBuilder.CreateField(recDef, "County");
			TransformDefinitionBuilder.CreateField(recDef, "PostCode");
			TransformDefinitionBuilder.CreateField(recDef, "AddressCountryCode");
			TransformDefinitionBuilder.CreateField(recDef, "MainTelephoneCountryCode");
			TransformDefinitionBuilder.CreateField(recDef, "MainTelephoneAreaCode");
			TransformDefinitionBuilder.CreateField(recDef, "MainTelephoneSubscriberNumber");
			TransformDefinitionBuilder.CreateField(recDef, "MainFaxCountryCode");
			TransformDefinitionBuilder.CreateField(recDef, "MainFaxAreaCode");
			TransformDefinitionBuilder.CreateField(recDef, "MainFaxSubscriberNumber");
			TransformDefinitionBuilder.CreateField(recDef, "MainWebSite");
			TransformDefinitionBuilder.CreateField(recDef, "DefaultTaxCode", IntManFieldTypeEnum.eLongInt);
			TransformDefinitionBuilder.CreateField(recDef, "TaxRegistrationCode");
			TransformDefinitionBuilder.CreateField(recDef, "MonthsToKeepTransactions", IntManFieldTypeEnum.eLongInt);
			TransformDefinitionBuilder.CreateField(recDef, "OrderPriority");
			TransformDefinitionBuilder.CreateField(recDef, "DUNSCode");
			TransformDefinitionBuilder.CreateField(recDef, "UseTaxCodeAsDefault", IntManFieldTypeEnum.eBoolean);
			TransformDefinitionBuilder.CreateField(recDef, "DefaultNominalCode");
			TransformDefinitionBuilder.CreateField(recDef, "TradingAccountType");
			TransformDefinitionBuilder.CreateField(recDef, "EarlySettlementDiscountPercent", IntManFieldTypeEnum.eDecimal);
			TransformDefinitionBuilder.CreateField(recDef, "EarlySettlementDiscountDays", IntManFieldTypeEnum.eLongInt);
			TransformDefinitionBuilder.CreateField(recDef, "PaymentTermsDays", IntManFieldTypeEnum.eLongInt);
			TransformDefinitionBuilder.CreateField(recDef, "PaymentTermsBasis", IntManFieldTypeEnum.eLongInt);
			TransformDefinitionBuilder.CreateField(recDef, "SAGE200IMPSUCCESS");

			return tfmDef;
		}
	}
}
