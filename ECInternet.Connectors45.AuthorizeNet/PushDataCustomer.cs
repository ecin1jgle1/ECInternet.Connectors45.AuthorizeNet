using System;
using Realisable.Data.Transform;
using Realisable.Resources;
using Realisable.Utils.DTO;

namespace ECInternet.Connectors45.Authorize_Net
{
	/// <summary>
	/// Inserts/updates customer records. This class demonstrates a simple import using a flat
	/// (non-hierarchical) dataset.
	/// </summary>
	class PushDataCustomer : IImportProcess, IDisposable
	{
		private const string ENTITY = "Customer";
		private const string ENTITY_DESC = "Customer";
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
		/// <param name="auditController">The audit controller.</param>
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
					Transaction trn = _iterator.Item();

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

		/// <summary>
		/// Updates/Inserts the customer record
		/// </summary>
		/// <param name="transaction">The transaction being processed.</param>
		private void CreateEntry(Transaction transaction)
		{
			bool isNew = false;

			// A mock call to whatever API.
			// Essentially get an object so the values of the dataset can be set to it.
			ApplicationObject customer = ApplicationObject.Create();

			try
			{
				// Use a common function in your wrapper/helper class to determine if a customer exists or not.
				isNew = !_wrapper.IsExistingCustomer(transaction.GetValueNullReturn("Reference") as string);

				// Now check/compare the updateflags, if the relevant flag is not set, return.
				if (isNew)
				{
					if ((_updateOp & eERPUpdateOperation.eInsert) != eERPUpdateOperation.eInsert)
					{
						return;
					}
				}
				else
				{
					if ((_updateOp & eERPUpdateOperation.eUpdate) != eERPUpdateOperation.eUpdate)
					{
						return;
					}
				}

				// Set the fields from the transaction onto the customer/application object.
				_wrapper.SetField(customer, "Name", transaction);
				_wrapper.SetField(customer, "ShortName", transaction);
				_wrapper.SetFieldDecimal(customer, "CreditLimit", transaction, 2);
				_wrapper.SetField(customer, "PaymentTermsDays", transaction);
				_wrapper.SetField(customer, "PaymentTermsBasis", transaction);
				_wrapper.SetField(customer, "CountryCode", transaction);
				_wrapper.SetField(customer, "AddressLine1", transaction);
				_wrapper.SetField(customer, "AddressLine2", transaction);
				_wrapper.SetField(customer, "AddressLine3", transaction);
				_wrapper.SetField(customer, "AddressLine4", transaction);
				_wrapper.SetField(customer, "City", transaction);
				_wrapper.SetField(customer, "County", transaction);
				_wrapper.SetField(customer, "Postcode", transaction);
				// Continue for each field until done.

				// Use the wrapper/helper class to set user defined/dynamic fields.
				_wrapper.SetUserDefinedFields(customer, transaction);

				// Now update the record...obviously it's going to be more compex than this.
				customer.Update();

				// In this example the customer reference is generated by the application...we may want to allow
				// this auto-generated reference to be recorded back onto the dataset to allow either:
				// a. Further processing i.e. updating a system with a foreign reference or;
				// b. Allow the value to be put onto the audit report.
				// 
				// So, if the transaction object has a Reference field update the field with the value.
				// Note, if you attempt to set a field which is not defined an exception is generated.
				if (_tranHasReferenceFld)
				{
					transaction.SetFieldValue("Reference", customer.GetFieldValue("Reference"), true);

				}

				// Now update the AuditController
				if (isNew)
				{
					_wrapper.AuditController.OnInsert(_transformId, transaction);
				}
				else
				{
					_wrapper.AuditController.OnUpdate(_transformId, transaction);
				}
			}
			catch (Exception ex)
			{
				string errorText = string.Format("Error whilst processing {0} record.", ENTITY_DESC);

				// There's an error based on the error action.
				switch (_errAction)
				{
					// If the error action is set to Abort: Log the error and re-raise the exception.
					case eTransformErrorAction.eTransformErrorActionAbort:
						{
							_wrapper.LogError(transaction, ex, string.Format("{0}  The entry will be discarded.", errorText));

							throw;
						}
					// If the error action is set to Continue: CHAOS mode.
					// Try to save/update again and see what happens.
					// An exception will probably be generated, causing the transform to terminate unexpectedly.
					case eTransformErrorAction.eTransformErrorActionContinue:
						{
							_wrapper.LogError(transaction, ex, string.Format("{0}  The entry will attempt to re-save.", errorText));
							//customer.Update();

							if (isNew)
							{
								_wrapper.AuditController.OnInsert(_transformId, transaction);

							}
							else
							{
								_wrapper.AuditController.OnUpdate(_transformId, transaction);
							}

							break;
						}
					// If the error action is Reject Record: Log the error and no update will occur.
					// Exit the method gracefully so the next record can be processed.
					case eTransformErrorAction.eTransformErrorActionRejectRecord:
						{
							_wrapper.LogError(transaction, ex, string.Format("{0}  The entry will be discarded.", errorText));

							break;
						}
				}
			}
		}

		/// <summary>
		/// Caches any values/fields to be used during processing.
		/// </summary>
		/// <param name="transaction">The transaction object.</param>
		private void CacheTransactionFields(Transaction transaction)
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

		~PushDataCustomer()
		{
			Dispose(false);
		}
	}
}
