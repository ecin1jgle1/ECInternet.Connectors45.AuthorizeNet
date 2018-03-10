using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using Realisable.Data.Transform;
using Realisable.Resources;
using Realisable.Utils.DTO;

using AuthorizeNet;

namespace ECInternet.Connectors45.Authorize_Net
{
	public class PushDataAuthorize : IImportProcess, IDisposable
	{
		private const string ENTITY = "Authorize";
		private const string ENTITY_DESC = "Authorize";
		private const string ENTITY_ID_FIELD = "Id";

		private string _transformId;
		private TransactionIterator _iterator;
		private eERPUpdateOperation _updateOp;
		private eTransformErrorAction _errAction;
		private ImportHelper _wrapper;
		private SystemConnectorDTO _system;
		private ITransformAuditController _auditController;

		bool _tranHasReferenceFld;

		#region IProcess Members

		/// <summary>
		/// Initialises the import.
		/// </summary>
		/// <param name="transformId">The transform id.</param>
		/// <param name="iterator">The transaction iterator object.</param>
		/// <param name="updateOperation">The update operation for the import.</param>
		/// <param name="errorAction">The error action.</param>
		/// <param name="wrapper">The wrapper object.</param>
		public void Initialise(string transformId, TransactionIterator iterator, eERPUpdateOperation updateOperation, eTransformErrorAction errorAction, ImportHelper wrapper, SystemConnectorDTO system, ITransformAuditController auditController)
		{
			_transformId = transformId;
			_iterator = iterator;
			_updateOp = updateOperation;
			_errAction = errorAction;
			_wrapper = wrapper;
			_system = system;
			_auditController = auditController;
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
						CacheTransactionFields(trn);

						// This shouldn't ever fail in live, but when testing it's easy to create the data structures incorrectly
						// and you're left scratching your head why it isn't working.
						if (trn.TransactionId != ENTITY)
						{
							throw new Exception(string.Format("Invalid Data Structure. The {0} Import does not contain a [{1}] record.", ENTITY_DESC, ENTITY));
						}
					}

					// Be a good citizen and call the OnProcess method on the AuditController object. This updates the PROCESSED record count, which can then be used in the Audit log.
					_wrapper.AuditController.OnProcess(_transformId, trn);

					// Now create the entry.
					CreateEntry(trn);

					i++;
				}
			}
		}

		#endregion

//		public void Authorize()
//		{
//			//pull from the store
//			AuthorizeNetCommon authnetCommon = new AuthorizeNetCommon();
//			var gate = authnetCommon.OpenGateway();

//			//build the request from the Form post
//			//var apiRequest = CheckoutFormReaders.BuildAuthAndCaptureFromPost();
//			//validate the request first

//			NameValueCollection post = new NameValueCollection();
//			var apiRequest = new AuthorizationRequest(post);

//			var api = new ApiFields();
//			foreach (string item in post.Keys)
//			{

//				//always send the keys to the API - this allows for Merchant Custom Keys
//				apiRequest.Queue(item, post[item]);
//			}

//			//set the amount - you can also set this from the page itself
//			//you have to have a field named "x_amount"
//			apiRequest.Queue(ApiFields.Amount, "1.23");
//			//You can set your solution ID here.
//			apiRequest.SolutionID = "AAA100302";

//			//send to Auth.NET
//			var response = gate.Send(apiRequest);

//			// Example from Randy (from PushDataCustomer)
//			//apiResponse = apiRequest.Request;
//			//transaction.SetFieldValue("Response", apiResponse.GetFieldValue("Response"), true);

//			string authCode;
//			string transactionID;
//			string orderMessage;
//			if (response.Approved)
//			{
//				authCode = response.AuthorizationCode;
////				transaction.SetFieldValue("AuthCode", 
//				transactionID = response.TransactionID;
//				orderMessage = string.Format("Thank you! Order approved: {0}", response.AuthorizationCode);
//			}
//			else
//			{
//				//error... oops. Reload the page
//				orderMessage = response.Message;
//			}
//		}

		/// <summary>
		/// Updates/Inserts the customer record
		/// </summary>
		/// <param name="transaction">The transaction being processed.</param>
		private void CreateEntry(Realisable.Data.Transform.Transaction transaction)
		{

			// A mock call to whatever API.
			// Essentially get an object so the values of the dataset can be set to it.
			AuthorizeNetCommon authnetCommon = new AuthorizeNetCommon();
			try
			{
				//pull from the store
				var gate = authnetCommon.OpenGateway();

				//build the request from the Form post
				//var apiRequest = CheckoutFormReaders.BuildAuthAndCaptureFromPost();
				//validate the request first

				NameValueCollection post = new NameValueCollection();
				var apiRequest = new AuthorizationRequest(post);

				var api = new ApiFields();
				foreach (string item in post.Keys)
				{

					//always send the keys to the API - this allows for Merchant Custom Keys
					apiRequest.Queue(item, post[item]);
				}
				apiRequest.Queue(ApiFields.Amount, transaction.GetValue("Amount").ToString());
				apiRequest.Queue(ApiFields.SolutionID, transaction.GetValue("SolutionID").ToString());
				//set the amount - you can also set this from the page itself
				//you have to have a field named "x_amount"
				//				apiRequest.Queue(ApiFields.Amount, "1.23");
				//You can set your solution ID here.
				//apiRequest.SolutionID = "AAA100302";


				//send to Auth.NET
				var response = gate.Send(apiRequest);

				// Example from Randy (from PushDataCustomer)
				//apiResponse = apiRequest.Request;
				transaction.SetFieldValue("ResponseCode", response.ResponseCode, true);
				transaction.SetFieldValue("Approved", response.Approved, true);
				transaction.SetFieldValue("Message", response.Message, true);

				if (response.Approved)
				{
					transaction.SetFieldValue("TransactionID", response.TransactionID, true);
					transaction.SetFieldValue("AuthorizationCode", response.AuthorizationCode, true);
				}
				else
				{
					//error... oops. Reload the page
					throw new Exception(response.Message);
				}

				// Now update the AuditController
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
						//customer.Update();

						_wrapper.AuditController.OnInsert(_transformId, transaction);

						break;
					// If the error action is Reject Record: Log the error and no update will occur.
					// Exit the method gracefully so the next record can be processed.
					case eTransformErrorAction.eTransformErrorActionRejectRecord:
						_wrapper.LogError(transaction, ex, string.Format("{0}  The entry will be discarded.", errorText));

						break;
				}
			}
		}

		/// <summary>
		/// Caches any values/fields to be used during processing.
		/// </summary>
		/// <param name="transaction">The transaction object.</param>
		private void CacheTransactionFields(Realisable.Data.Transform.Transaction transaction)
		{
			// Cache the fields to be used later on.
			_tranHasReferenceFld = transaction.HasField("Reference");
		}

		private void DisposeObj(IDisposable obj)
		{
			if (obj != null)
			{
				obj.Dispose();
				obj = null;
			}
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
				// Our application object doesn't need disposing, but some may!
				// DisposeObj(_wrapper);
			}
		}

		#endregion

		~PushDataAuthorize()
		{
			Dispose(false);
		}

	}

}
