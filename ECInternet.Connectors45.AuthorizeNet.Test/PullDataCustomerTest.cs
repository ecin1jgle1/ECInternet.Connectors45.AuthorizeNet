using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Realisable.Data.Transform;
using Realisable.JobDefinition;
using Realisable.Resources;
using Realisable.Utils.DTO;

namespace ECInternet.Connectors45.Authorize_Net.Test
{
	/// <summary>
	/// Test class illustrating how to test a flat dataset for a pull style connector.
	/// </summary>
	[TestClass]
	public class PullDataCustomerTest : ConnectorTest
	{
		private bool _testMode = true;

		/// <summary>
		/// Gets or sets the test context which provides information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }

		/// <summary>
		/// Perfoms a test to iterate a flat dataset for a pull style connector.
		/// </summary>
		[TestMethod, Ignore]
		public void FlatPullConnectorTest()
		{
			TransformDefinition tfmDef = CreateCustomerDefintion();

			IReader reader = new PullDataInterop();
			reader.Initialise(tfmDef, _testMode, new SystemConnectorDTO(TestConstants.CONNECTOR_SYSTEM));

			IterateFlat(tfmDef.RecordDefById("Customer"), reader);
		}

		private void IterateFlat(TransformRecordDefinition recDef, IReader reader)
		{
			string[] fields = recDef.FieldIds().ToArray();

			for(int i = 0; i < reader.RecordCount; i++)
			{
				reader.MoveNext();
				string[] vals = reader.GetValues(ref fields);

				// TODO: Here you would test the output of the fields to check if they 
				// are the result you expect.
				// Instead of output'ing to the Console, you would use Assert.

				string output = string.Empty;
				for (int j = 0; j < vals.Length; j++)
				{
					output += vals[j] + ", ";
				}

				Debug.WriteLine("Record #" + i + " - " + output);
			}
		}

		/// <summary>
		/// Creates the TransformDefinition for the dataset to be iterated.
		/// </summary>
		private TransformDefinition CreateCustomerDefintion()
		{
			// Create the resultant object.
			TransformDefinition result = CreatePullStyleTransformDef(TestConstants.CONNECTOR_SYSTEM, "Customer");

			TransformRecordDefinition customerRecDef = result.CreateTransformRecord("Customer", string.Empty);

			TransformDefinitionBuilder.CreateField(customerRecDef, "AccountNo");
			TransformDefinitionBuilder.CreateField(customerRecDef, "AccountName");
			TransformDefinitionBuilder.CreateField(customerRecDef, "CreditLimit", IntManFieldTypeEnum.eDecimal);
			TransformDefinitionBuilder.CreateField(customerRecDef, "Currency");
			TransformDefinitionBuilder.CreateField(customerRecDef, "AddressLine1");
			TransformDefinitionBuilder.CreateField(customerRecDef, "AddressLine2");
			TransformDefinitionBuilder.CreateField(customerRecDef, "AddressLine3");
			TransformDefinitionBuilder.CreateField(customerRecDef, "AddressLine4");
			TransformDefinitionBuilder.CreateField(customerRecDef, "City");
			TransformDefinitionBuilder.CreateField(customerRecDef, "County");
			TransformDefinitionBuilder.CreateField(customerRecDef, "Country");
			TransformDefinitionBuilder.CreateField(customerRecDef, "PostCode");
			TransformDefinitionBuilder.CreateField(customerRecDef, "DefaultTaxCode", IntManFieldTypeEnum.eLongInt);
			TransformDefinitionBuilder.CreateField(customerRecDef, "OrderPriority");
			TransformDefinitionBuilder.CreateField(customerRecDef, "UseTaxCodeAsDefault", IntManFieldTypeEnum.eBoolean);

			return result;
		}
	}
}
