using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Realisable.Data.Transform;
using Realisable.JobDefinition;
using Realisable.Resources;
using Realisable.Utils.DTO;

namespace ECInternet.Connectors45.Authorize_Net.Test
{
	/// <summary>
	/// Test class illustrating how to test a hierarchical dataset for a pull style connector.
	/// </summary>
	[TestClass]
	public class PullDataOrderTest : ConnectorTest
	{
		private bool _testMode = true;

		/// <summary>
		/// Gets or sets the test context which provides information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }

		/// <summary>
		/// Entry method for calling hierarchical test.
		/// </summary>
		[TestMethod, Ignore]
		public void HierarchicalPullConnectorTest()
		{
			TransformDefinition tfmDef = CreateOrderDefintion();

			IReader reader = new PullDataInterop();
			reader.Initialise(tfmDef, _testMode, new SystemConnectorDTO(TestConstants.CONNECTOR_SYSTEM));

			IterateRecursive(tfmDef.RecordDefById("Order"), reader);
		}

		/// <summary>
		/// This method performs the same calls as in a live scenario.
		/// I.e. the MoveXXX calls are the same ones called by IMan to a CustomReader.
		/// </summary>
		/// <param name="recDef">The TransformRecordDefinition of the current object being iterated.</param>
		/// <param name="reader">The IReader object being iterated.</param>
		private void IterateRecursive(TransformRecordDefinition recDef, IReader reader)
		{
			string[] fields = recDef.FieldIds().ToArray();

			for (int i = 0; i < reader.RecordCount; i++)
			{
				reader.MoveNext();
				string[] vals = reader.GetValues(ref fields);

				// TODO: Here you would test the output of the fields to check if they are the result you expect.
				// Instead of output'ing to the Console, you would use Assert.

				string output = string.Empty;
				for (int j = 0; j < vals.Length; j++)
				{
					output += vals[j] + ", ";
				}
				Debug.WriteLine("RecordDef - " + recDef.RecordId + " - Record #" + i + " - " + output);

				// Recursively iterate the dataset.
				foreach (TransformRecordDefinition childRecDef in recDef.ChildrenRecords)
				{
					// Move the Reader to the Child object.
					reader.MoveChild(childRecDef.RecordId);

					// Recurse.
					IterateRecursive(childRecDef, reader);

					// Now move the Reader to the Parent.
					reader.MoveParent();
				}
			}
		}

		/// <summary>
		/// Creates the TransformDefinition for the dataset to be iterated.
		/// </summary>
		private TransformDefinition CreateOrderDefintion()
		{
			// Create the resultant object.
			TransformDefinition result = CreatePullStyleTransformDef(TestConstants.CONNECTOR_SYSTEM, "Order");

			TransformRecordDefinition orderRecDef = result.CreateTransformRecord("Order", string.Empty);
			TransformDefinitionBuilder.CreateField(orderRecDef, "UniqueID");
			TransformDefinitionBuilder.CreateField(orderRecDef, "OrderType", IntManFieldTypeEnum.eLongInt);
			TransformDefinitionBuilder.CreateField(orderRecDef, "Customer");
			TransformDefinitionBuilder.CreateField(orderRecDef, "OrderDate", IntManFieldTypeEnum.eDate);
			TransformDefinitionBuilder.CreateField(orderRecDef, "ShipDate", IntManFieldTypeEnum.eDate);
			TransformDefinitionBuilder.CreateField(orderRecDef, "OrderNo");
			TransformDefinitionBuilder.CreateField(orderRecDef, "ExchangeRate", IntManFieldTypeEnum.eDecimal);
			TransformDefinitionBuilder.CreateField(orderRecDef, "NetValue", IntManFieldTypeEnum.eDecimal);
			TransformDefinitionBuilder.CreateField(orderRecDef, "DiscountValue", IntManFieldTypeEnum.eDecimal);
			TransformDefinitionBuilder.CreateField(orderRecDef, "TaxValue", IntManFieldTypeEnum.eDecimal);

			TransformRecordDefinition orderDetailRecDef = result.CreateTransformRecord("OrderDetail", "Order");
			TransformDefinitionBuilder.CreateField(orderDetailRecDef, "Item");
			TransformDefinitionBuilder.CreateField(orderDetailRecDef, "Description", IntManFieldTypeEnum.eLongInt);
			TransformDefinitionBuilder.CreateField(orderDetailRecDef, "Qty", IntManFieldTypeEnum.eDecimal);
			TransformDefinitionBuilder.CreateField(orderDetailRecDef, "UnitAmount", IntManFieldTypeEnum.eDecimal);
			TransformDefinitionBuilder.CreateField(orderDetailRecDef, "TaxCode", IntManFieldTypeEnum.eLongInt);
			TransformDefinitionBuilder.CreateField(orderDetailRecDef, "DiscountAmount", IntManFieldTypeEnum.eDecimal);
			TransformDefinitionBuilder.CreateField(orderDetailRecDef, "TaxAmount", IntManFieldTypeEnum.eDecimal);

			TransformRecordDefinition serialNumRecDef = result.CreateTransformRecord("SerialNumbers", "OrderDetail");

			TransformDefinitionBuilder.CreateField(serialNumRecDef, "Serial");

			return result;
		}
	}
}
