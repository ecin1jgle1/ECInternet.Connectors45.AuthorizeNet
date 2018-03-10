using System;
using Realisable.Data.Transform;
using Realisable.Resources;
using Realisable.Utils.DTO;

namespace ECInternet.Connectors45.Authorize_Net
{
	/// <summary>
	/// This class demonstrates how to process heirarchical data...i.e. header/detail structures.
	/// </summary>
	class PushDataSalesInvoice : IImportProcess, IDisposable
	{
		private const string ENTITY = "SalesInvoice";
		private const string ENTITY_DESC = "Sales Invoice";
		private const string ENTITY_DETAIL = "SalesInvoiceDetail";
		private const string ENTITY_DETAIL_DESC = "Sales Invoice Detail";

		private string _transformId;
		private TransactionIterator _iterator;
		private eERPUpdateOperation _updateOp;
		private eTransformErrorAction _errAction;
		private ImportHelper _wrapper;
		private SystemConnectorDTO _system;
		private ITransformAuditController _auditController;

		private bool _tranHasURNFld;
		private bool _tranHasPostedDocTotalFld;

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

		private void CreateEntry(Transaction transaction)
		{
			ApplicationObject salesInvoice = ApplicationObject.Create();

			try
			{
				// Set the customer for the Sales Invoice
				string customerId = transaction.GetValueNullReturn("Customer") as string;
				if (!_wrapper.IsExistingCustomer(customerId))
				{
					throw new Exception(string.Format("The customer [{0}] is invalid", customerId), null);
				}

				_wrapper.SetField(salesInvoice, "Customer", transaction);
				_wrapper.SetField(salesInvoice, "InstrumentNo", transaction);
				_wrapper.SetField(salesInvoice, "SecondReference", transaction);
				_wrapper.SetField(salesInvoice, "InstrumentDate", transaction);
				_wrapper.SetField(salesInvoice, "DueDate", transaction);

				// In this section the detail lines of the invoice are iterated.
				// Firstly check the iterator contains any child records.
				if (_iterator.HasChild(ENTITY_DETAIL))
				{
					// Now move to the child record.
					if (_iterator.MoveChild(ENTITY_DETAIL))
					{
						int detailCnt = 0;

						// The iterator is now positioned on the child record.
						// Create a detail record for each of the child records.
						while (_iterator.Read())
						{
							detailCnt++;
							CreateDetailEntry(salesInvoice, _iterator.Item());
						}

						// Now move back to the parent.
						_iterator.MoveParent();
					}
				}

				// Set any remaing fields that may need to be set after the detail records.
				_wrapper.SetFieldDecimal(salesInvoice, "DicountPercent", transaction, 2);
				_wrapper.SetField(salesInvoice, "DiscountDays", transaction);

				// Update the invoice.
				salesInvoice.Update();

				_wrapper.AuditController.OnInsert(_transformId, transaction);

				// Post update we may need to update the recordset with a number of fields. 
				if (_tranHasURNFld)
				{
					transaction.SetFieldValue("PostingReference", salesInvoice.GetFieldValue("PostRef"), true);
				}

				// Write the posted document total onto the transaction.
				if (_tranHasPostedDocTotalFld)
				{
					transaction.SetFieldValue("DocumentTotal", salesInvoice.GetFieldValue("DocTotal"), true);
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
							salesInvoice.Update();

							// Post update we may need to update the recordset with a number of fields. 
							if (_tranHasURNFld)
							{
								transaction.SetFieldValue("PostingReference", salesInvoice.GetFieldValue("PostRef"), true);
							}

							//Write the posted document total onto the transaction.
							if (_tranHasPostedDocTotalFld)
							{
								transaction.SetFieldValue("DocumentTotal", salesInvoice.GetFieldValue("DocTotal"), true);
							}

							_wrapper.AuditController.OnInsert(_transformId, transaction);

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
			finally
			{
				// Check the iterator is back at the header; move the parent if it is not.
				if (_iterator.Item().TransactionId != ENTITY)
				{
					_iterator.MoveParent();
				}
			}
		}

		private void CreateDetailEntry(ApplicationObject salesInvoice, Transaction transaction)
		{
			ApplicationObject detailItem = ApplicationObject.Create();

			_wrapper.AuditController.OnProcess(_transformId, transaction);

			try
			{
				_wrapper.SetField(detailItem, "NominalSpecification", transaction);
				_wrapper.SetField(detailItem, "Narrative", transaction);
				_wrapper.SetFieldDecimal(detailItem, "Amount", transaction, 2);
				_wrapper.SetField(detailItem, "TransactionAnalysisCode", transaction);

				_wrapper.AuditController.OnInsert(_transformId, transaction);

			}
			catch (Exception ex)
			{
				string errorText = string.Format("Error whilst processing {0} record.", ENTITY_DETAIL_DESC);

				_wrapper.LogError(transaction, ex, string.Format("{0}  The detail record will be discarded.", errorText));

				switch (_errAction)
				{
					case eTransformErrorAction.eTransformErrorActionRejectRecord:
					case eTransformErrorAction.eTransformErrorActionAbort:
						{
							throw;
						}
					case eTransformErrorAction.eTransformErrorActionContinue:
						{
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
			// Posting Reference
			_tranHasURNFld = transaction.HasField("PostingReference");
			// Document Total
			_tranHasPostedDocTotalFld = transaction.HasField("DocumentTotal");
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

		~PushDataSalesInvoice()
		{
			Dispose(false);
		}
	}
}
