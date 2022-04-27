using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Realisable.Data.Interop;
using Realisable.Data.Transform;
using Realisable.JobDefinition;
using Realisable.Resources;
using Realisable.Utils;
using Realisable.Utils.DTO;
using Realisable.Utils.OM;

namespace ECInternet.Connectors45.Authorize_Net.Test
{
	/// <summary>
	/// Provides an abstract class for instantiating and processing connector datasets.
	/// Every test should inherit from this class.
	/// </summary>
	[TestClass]
	public abstract class ConnectorTest
	{
		/// <summary>
		/// The id of the Job Definition created as part of the unit tests.
		/// </summary>
		public const string JOBDEF_ID = "CONNECTORTEST";

		/// <summary>
		/// The name of the Job Definition created as part of the unit tests.
		/// </summary>
		public const string JOBDEF_NAME = "ConnectorTest";

		/// <summary>
		/// A number used to signify the instance of the test. Leave as default as it a placeholder used in a live scenario.
		/// </summary>
		public const int INSTANCE_ID = 9977;

		protected JobDefinition _jobDef;

		[TestInitialize]
		public void TestInitialise()
		{
			// Create the JobDefinition object so it's available to all.
			_jobDef = CreateJobDef();

			// Setup the NHibernate session so data can be from the IMan database.
			SessionBuilderFactory.GetBuilder(SessionBuilderType.NonHttp).OpenSession();
		}

		[TestCleanup]
		public void TestClose()
		{
			SessionBuilderFactory.GetBuilder(SessionBuilderType.NonHttp).CloseSession();
		}

		/// <summary>
		/// Creates a JobDefinition so the Connector's transformDefiniton can be added.
		/// </summary>
		/// <returns></returns>
		public JobDefinition CreateJobDef()
		{
			var result = new JobDefinition();
			result.JobName = JOBDEF_NAME;
			return result;
		}

		/// <summary>
		/// Creates a TransformDefinition object for a Push Style Connector.
		/// </summary>
		/// <param name="systemConnectorId">The id for the System Connector used to retrieve the connection properties.</param>
		/// <param name="impType">The import type.</param>
		/// <param name="updateOp">The update operation.</param>
		/// <returns></returns>
		protected TransformDefinition CreatePushStyleTransformDef(string systemConnectorId, ImportTypeEnum impType, eERPUpdateOperation updateOp)
		{
			TransformDefinition tfmDef = _jobDef.CreateTransform();
			ITransformSetup tfmSetup = new CustomConnectorTransformSetup();
			tfmSetup.InitialiseTransform(tfmDef);

			// Set the options for the connector.
			tfmDef.Options.SetProperty(GlobalResources.SYS_CONNECT_TRAN_TYPE_OPT, Convert.ToInt32(impType).ToString(), false);
			tfmDef.Options.SetProperty(GlobalResources.SYS_CONNECT_SYSID_OPT, systemConnectorId, false);
			tfmDef.Options.SetProperty(GlobalResources.SYS_CONNECT_UPDATE_OP_OPT, Convert.ToInt32(updateOp).ToString(), false);
			tfmDef.Options.SetProperty(GlobalResources.CUS_CONNECT_ASSEMBLY_NAME_OPT, TestConstants.ASSEMBLY_NAME, false);

			tfmDef.AuditDefinition.ErrorAction = eTransformErrorAction.eTransformErrorActionAbort;

			return tfmDef;
		}

		/// <summary>
		/// Creates a TransformDefinition object for a Pull Style connector.
		/// </summary>
		/// <param name="systemConnectorId">The id for the System Connector used to retrieve the connection properties.</param>
		/// <param name="importDataType">The type of data being retrieved.</param>
		/// <returns>The constructed TransformDefinition object.</returns>
		protected TransformDefinition CreatePullStyleTransformDef(string systemConnectorId, string importDataType, eTransformErrorAction errorAction = eTransformErrorAction.eTransformErrorActionAbort)
		{
			TransformDefinition result = _jobDef.CreateTransform();

			ITransformSetup tfmSetup = new CustomReaderSetup();
			tfmSetup.InitialiseTransform(result);

			// Set the options for the Reader definition. 
			result.Options.SetProperty(GlobalResources.SYS_CONNECT_TRAN_TYPE_OPT, importDataType, false);
			result.Options.SetProperty(GlobalResources.SYS_CONNECT_SYSID_OPT, systemConnectorId, false);
			result.Options.SetProperty(GlobalResources.CUS_CONNECT_ASSEMBLY_NAME_OPT, TestConstants.ASSEMBLY_NAME, false);

			// Set any custom options. Move this to your actual test code if the options are specific to a test.
			result.Options.SetProperty("Option1", "Option Value 1", false);
			result.Options.SetProperty("Option2", "Option Value 2", false);

			result.AuditDefinition.ErrorAction = errorAction;

			return result;
		}

		/// <summary>
		/// Processes a Push Style dataset by instantiating the ITransformInterop object and making the appropriate calls.
		/// </summary>
		/// <param name="transactionFactory">The transaction factory containing the dataset.</param>
		//protected string ProcessTransactions(TransactionFactory transactionFactory)
		//{
		//	ITransformInterop import = new TransformInterop();
		//	import.CreateController(JOBDEF_ID, INSTANCE_ID, transactionFactory.TransformDefinition.GetXml(), transactionFactory.Data.OuterXml, true, string.Empty);
		//	((TransformInterop) import).SetAuditProxy(new AuditProxy());

		//	// Run the actual import.
		//	import.Process();

		//	return import.TransformedData();
		//}
		protected static void ProcessTransactions(TransactionFactory transactionFactory)
		{
			TransformDefinition tfmDef = transactionFactory.TransformDefinition;

			// Obtain the system connector.
			SystemConnectorDTO connectorDTO = new SystemConnectorDTO(tfmDef.Options.GetProperty(GlobalResources.SYS_CONNECT_SYSID_OPT));

			// Get the updateOperation.
			eERPUpdateOperation updateOperation = tfmDef.Options.GetProperty<eERPUpdateOperation>(GlobalResources.SYS_CONNECT_UPDATE_OP_OPT);

			// Create the ITransformProcess object implemented by the connector.
			PushDataInterop target = new PushDataInterop();

			// Now initialise.
			target.Initialise(tfmDef, transactionFactory.CreateTransactionIterator(), false, tfmDef.AuditDefinition.ErrorAction, updateOperation, connectorDTO, new AuditProxy());

			// Run the actual import.
			target.Process();
		}

		protected TransactionIterator ProcessTransactions(TransactionFactory transactionFactory, TransformDefinition tfmDef, SystemConnectorDTO system)
		{
			TransactionIterator iterator = new TransactionIterator(transactionFactory.Data, tfmDef);

			PushDataInterop target = new PushDataInterop();
			target.Initialise(tfmDef, iterator, true, eTransformErrorAction.eTransformErrorActionAbort, eERPUpdateOperation.eUpdateInsert, system, new AuditProxy());

			// Run the actual import.
			target.Process();

			return iterator;
		}

		protected TransactionIterator ProcessTransactions(TransactionFactory transactionFactory, TransformDefinition tfmDef, eERPUpdateOperation updateOperation, SystemConnectorDTO system)
		{
			TransactionIterator iterator = new TransactionIterator(transactionFactory.Data, tfmDef);

			PushDataInterop target = new PushDataInterop();
			target.Initialise(tfmDef, iterator, true, eTransformErrorAction.eTransformErrorActionAbort, updateOperation, system, new AuditProxy());

			// Run the actual import.
			target.Process();

			return iterator;
		}


		/// <summary>
		/// Process a Push Style dataset where the data contains a known error. Used to test exceptional test cases.
		/// </summary>
		/// <param name="transactionFactory">The transaction factory containing the dataset.</param>
		/// <typeparam name="T">The type of exception expected.</typeparam>
		//protected void ProcessExceptionalTransactions<T>(TransactionFactory transactionFactory) where T : Exception
		//{
		//	ITransformInterop import = new TransformInterop();
		//	import.CreateController(JOBDEF_ID, INSTANCE_ID, transactionFactory.TransformDefinition.GetXml(), transactionFactory.Data.OuterXml, false, string.Empty);
		//	((TransformInterop) import).SetAuditProxy(new AuditProxy());

		//	// Run the actual import using AssetExternsions.Throws method.
		//	AssertExtensions.Throws<T>(() => import.Process());
		//}
		protected static void ProcessExceptionalTransactions<T>(TransactionFactory transactionFactory) where T : Exception
		{
			// Run the actual import using AssetExternsions.Throws method.
			AssertExtensions.Throws<T>(() => ProcessTransactions(transactionFactory));
		}
	}
}
