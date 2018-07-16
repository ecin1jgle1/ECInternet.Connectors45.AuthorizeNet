using System;
using System.Collections.Generic;

using Realisable.Data;
using Realisable.Data.Transform;
using Realisable.Resources;
using Realisable.Utils.DTO;

using AuthorizeNet.Api.Contracts.V1;

namespace ECInternet.Connectors45.Authorize_Net
{
	public class PushDataCapture : IImportProcess
	{
		private const string ENTITY = "Capture";
		private const string ENTITY_DESC = "Authorize.Net Capture";

		private string _transformId;
		private TransactionIterator _iterator;
		private eERPUpdateOperation _updateOp;
		private eTransformErrorAction _errAction;
		private ImportHelper _wrapper;
		private SystemConnectorDTO _systemConnector;
		private ITransformAuditController _auditController;

		private Client _client;
		private List<string> _validStatus = new List<string>();

		#region IProcess Members

		/// <summary>
		/// Initialises the import.
		/// </summary>
		/// <param name="transformId">The transform id.</param>
		/// <param name="iterator">The transaction iterator object.</param>
		/// <param name="updateOperation">The update operation for the import.</param>
		/// <param name="errorAction">The error action.</param>
		/// <param name="helper">The wrapper object.</param>
		public void Initialise(string transformId, TransactionIterator iterator, eERPUpdateOperation updateOperation, eTransformErrorAction errorAction, ImportHelper helper, SystemConnectorDTO systemConnector, ITransformAuditController auditController)
		{
			_transformId = transformId;
			_iterator = iterator;
			_updateOp = updateOperation;
			_errAction = errorAction;
			_wrapper = helper;
			_systemConnector = systemConnector;
			_auditController = auditController;

			_client = new Client(systemConnector);
			_validStatus = null;// _client.GetValidOrderStatuses();
		}

		/// <summary>
		/// Processes the transactions.
		/// </summary>
		public void CreateTransactions()
		{
			// Check the first record 
			if (!_iterator.EOF)
			{
				int i = 0;

				// For each of the transactions.
				while (_iterator.Read())
				{
					Realisable.Data.Transform.Transaction trn = _iterator.Item();

					// On the first record do any caching and make any necessary checks on the data.
					if (i == 0)
					{
						// This shouldn't ever fail in live, but when testing it's easy to create the data structures incorrectly
						// and you're left scratching your head why it isn't working.
						if (trn.TransactionId != ENTITY)
						{
							throw new Exception(string.Format("Invalid Data Structure. The {0} Import does not contain a [{1}] record.", ENTITY_DESC, ENTITY));
						}
					}

					// Be a good citizen and call the OnProcess method on the AuditController object.
					// This updates the PROCESSED record count, which can then be used in the Audit log.
					_wrapper.AuditController.OnProcess(_transformId, trn);

					// Now create the entry.
					CreateEntry(trn);

					i++;
				}
			}
		}

		#endregion

		private void CreateEntry(Realisable.Data.Transform.Transaction transaction)
		{
			ApplicationObject auth = ApplicationObject.Create();
			try
			{
				if (TransactionHasStatusUpdateFields(transaction))
				{

					string ApiLoginID = _client._username;
					string ApiTransactionKey = _client._password;

					string _transactionAmount = transaction.GetValue("TransactionAmount").ToString();
					decimal transactionAmount = 0.00M;
					if (!decimal.TryParse(_transactionAmount, out transactionAmount))
					{
						throw new Exception(string.Format("Unable to parse transaction amount [{0}]", _transactionAmount));
					}
					string TransactionID = transaction.GetValue("TransactionID").ToString();

					Capture response = Client.Capture(ApiLoginID, ApiTransactionKey, transactionAmount, TransactionID);

					if (response != null)
					{
						TraceWriter.Write("Authorize.Net capture response:\n" + response.ToTraceOutput());
						if (transaction.HasField(nameof(Capture.ResponseCode)))
						{
							transaction.SetFieldValue(nameof(Capture.ResponseCode), response.ResponseCode, true);
						}
						if (transaction.HasField(nameof(Capture.MessageCode)))
						{
							transaction.SetFieldValue(nameof(Capture.MessageCode), response.MessageCode, true);
						}
						if (transaction.HasField(nameof(Capture.Description)))
						{
							transaction.SetFieldValue(nameof(Capture.Description), response.Description, true);
						}
						if (transaction.HasField(nameof(Capture.AuthCode)))
						{
							transaction.SetFieldValue(nameof(Capture.AuthCode), response.AuthCode, true);
						}
						if (transaction.HasField(nameof(Capture.ErrorCode)))
						{
							transaction.SetFieldValue(nameof(Capture.ErrorCode), response.ErrorCode, true);
						}
						if (transaction.HasField(nameof(Capture.CaptureErrorMessage)))
						{
							transaction.SetFieldValue(nameof(Capture.CaptureErrorMessage), response.CaptureErrorMessage, true);
						}
					}
					else
					{
						TraceWriter.Write("Authorize.Net capture response is NULL.");
					}

					// Being a good citizen.
					_auditController.OnUpdate(_transformId, transaction);
				}
				else
				{
					throw new Exception("'Id' and 'OrderStatus' must both be set.");
				}

				_wrapper.AuditController.OnInsert(_transformId, transaction);
			}
			catch (Exception ex)
			{
				string errorText = string.Format("Error while processing {0} record.", ENTITY_DESC);

				// There's an error based on the error action.
				switch (_errAction)
				{
					// If the error action is set to Abort: Log the error and re-raise the exception.
					case eTransformErrorAction.eTransformErrorActionAbort:
						_wrapper.LogError(transaction, ex, string.Format("{0}  The entry will be discarded.", errorText));

						throw;
					// If the error action is set to Continue: CHAOS mode.
					// Try to save/update again and see what happens.
					// An exception will probably be generated, causing the transform to terminate unexpectedly.
					case eTransformErrorAction.eTransformErrorActionContinue:
						_wrapper.LogError(transaction, ex, string.Format("{0}  The entry will attempt to re-save.", errorText));
						//auth.Update();

						_wrapper.AuditController.OnInsert(_transformId, transaction);

						break;
					// If the error action is Reject Record: Log the error and no update will occur.
					// Exit the method gracefully so the next record can be processed.
					case eTransformErrorAction.eTransformErrorActionRejectRecord:
						_wrapper.LogError(transaction, ex, string.Format("{0}  The entry will be discarded.", errorText));

						break;
				}
			}
			finally
			{
				// Check the iterator is back at the header; move the parent if it is not.
				if (_iterator.Item().TransactionId != ENTITY)
				{
					_iterator.MoveParent();
				}
			}
		}


		private bool TransactionHasStatusUpdateFields(Realisable.Data.Transform.Transaction transaction)
		{
			TraceWriter.Write(transaction.Data.InnerXml);
			return true;
			//return transaction.HasField("Id") &&
			//		 transaction.IsFieldSet("Id") &&
			//		 transaction.HasField("OrderStatus") &&
			//		 transaction.IsFieldSet("OrderStatus");
		}

	}
}
