using System;
using Realisable.Data.Transform;
using Realisable.JobDefinition;
using Realisable.Resources;
using Realisable.Utils;
using Realisable.Utils.DTO;

namespace ECInternet.Connectors45.Authorize_Net
{
	/// <summary>
	/// This class implements a typical pattern for an pushing data to an application webservice.
	/// </summary>
	/// <remarks>
	/// This pattern assumes that different data uploads/imports each require their own specific implementation.
	/// 
	/// If a purely generic upload algorithm can be achieved the CreateImportObject method, IProcess interface and the 
	/// implementations of IProcess are not required.
	/// 
	/// Generic uploads are very difficult to achieve; there are always a gotcha's requiring call-outs for specific 
	/// circumstances.
	/// </remarks>
	public class PushDataInterop : ITransformProcess, IDisposable
	{
		private TransformDefinition _transformDef;
		private TransactionIterator _iterator;
		private bool _testMode;
		private eTransformErrorAction _errAction;
		private eERPUpdateOperation _updateOperation;
		private SystemConnectorDTO _system;
		private ITransformAuditController _auditController;

		#region ITransformProcess Members

		/// <summary>
		/// Is called before processing is started; do any initialisation here.
		/// </summary>
		/// <param name="transformDef">The connector transform definition.</param>
		/// <param name="iterator">The transaction iterator.</param>
		/// <param name="testMode">Indicates if transform is being run in the designer/Preview mode.</param>
		/// <param name="errAction">The action to take if there's an error.</param>
		/// <param name="updateOperation">Whether to insert and/or update and/or delete.</param>
		/// <param name="system">The System object containing the settings.</param>
		/// <param name="auditController">The audit controller object.</param>
		public void Initialise(TransformDefinition transformDef, TransactionIterator iterator, bool testMode, eTransformErrorAction errAction, eERPUpdateOperation updateOperation, SystemConnectorDTO system, ITransformAuditController auditController)
		{
			_transformDef = transformDef;
			_iterator = iterator;
			_testMode = testMode;
			_errAction = errAction;
			_updateOperation = updateOperation;
			_system = system;
			_auditController = auditController;

			Logon(system);
		}

		public void Process()
		{
			IImportProcess import = null;

			try
			{
				ImportHelper wrapper = new ImportHelper(_transformDef.TransformId, _auditController);

				// Returns new instance of "PushData" class based upon requested entity.
				import = CreateImportObject();

				// Calls the Initialize method of "PushData" class.
				import.Initialise(_transformDef.TransformId, _iterator, _updateOperation, _errAction, wrapper, _system, _auditController);

				// Does processing work for "PushData" class.
				import.CreateTransactions();
			}
			finally
			{
				if (import != null && import is IDisposable)
				{
					((IDisposable)import).Dispose();
					import = null;
				}
			}
		}

		#endregion

		/// <summary>
		/// Creates the import object.
		/// </summary>
		/// <returns></returns>
		private IImportProcess CreateImportObject()
		{
			// Get the import type enum from the Transform Definition Options.
			ImportTypeEnum importType = Converter.ConvertTo<ImportTypeEnum>(_transformDef.Options.GetProperty<string>(GlobalResources.SYS_CONNECT_TRAN_TYPE_OPT));

			switch (importType)
			{
				case ImportTypeEnum.Authorize:
					return new PushDataAuthorize();
				case ImportTypeEnum.Capture:
					return new PushDataCapture();
				default:
					return null;
			}
		}

		/// <summary>
		/// Logon to the application.
		/// </summary>
		/// <param name="system">The system object.</param>
		private void Logon(SystemConnectorDTO system)
		{
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
		}

		#endregion

		~PushDataInterop()
		{
			Dispose(false);
		}
	}
}
