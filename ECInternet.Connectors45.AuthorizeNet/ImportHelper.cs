using System;
using System.Collections;
using System.Collections.Generic;
using Realisable.Data.Transform;

namespace ECInternet.Connectors45.Authorize_Net
{
	/// <summary>
	/// This class provides a set of helper routines to reduce the amount of work performed in the actual 
	/// import classes. 
	/// </summary>
	public class ImportHelper
	{
		string _transformId;
		Transaction _currentTran;

		public ImportHelper(string tfmId, ITransformAuditController _auditController)
		{
			_transformId = tfmId;
			AuditController = _auditController;
		}

		public ITransformAuditController AuditController { get; private set; }

		/// <summary>
		/// The set fields allow you to call a single function from the import classes, reducing the amount of
		/// work that you need to do to find out of a transaction 'Has' a field or whether the field is 'set'.
		/// </summary>
		/// <param name="appObject"></param>
		/// <param name="fieldName"></param>
		/// <param name="transaction"></param>
		public void SetField(ApplicationObject appObject, string fieldName, Transaction transaction)
		{
			try
			{
				_currentTran = transaction;

				if (CanFieldBeSet(transaction, fieldName))
				{
					SetField(appObject, fieldName, transaction.GetValue(fieldName));
				}
			}
			finally
			{
				_currentTran = null;
			}
		}

		public void SetField(ApplicationObject appObject, string fieldName, Transaction transaction, string tranFieldName)
		{
			try
			{
				_currentTran = transaction;

				if (CanFieldBeSet(transaction, tranFieldName))
				{
					SetField(appObject, fieldName, transaction.GetValue(tranFieldName));
				}
			}
			finally
			{
				_currentTran = null;
			}
		}

		public void SetField(ApplicationObject appObject, string fieldName, object value)
		{
			// Actually update the object....obviously in the simple example there's an actual 'SetFieldValue'
			// method. If however, you may need to set Properties on the object itself, we have a set of helper
			// routines in our Reflection library that allows you to set a property efficiently using reflection.
			// Contact Realisable to discuss.
			appObject.SetFieldValue(fieldName, value);
		}

		/// <summary>
		/// Decimal fields sometimes need to be handled differently. The two SetFieldDecimals provide the specialist implementation.
		/// </summary>
		/// <param name="appObject"></param>
		/// <param name="fieldName"></param>
		/// <param name="transaction"></param>
		/// <param name="decimalPlaces"></param>
		public void SetFieldDecimal(ApplicationObject appObject, string fieldName, Transaction transaction, int decimalPlaces)
		{
			try
			{
				_currentTran = transaction;

				if (CanFieldBeSet(transaction, fieldName))
				{
					object val = transaction.GetValue(fieldName);

					SetFieldDecimal(appObject, fieldName, val, decimalPlaces);
				}
			}
			finally
			{
				_currentTran = null;
			}
		}

		public void SetFieldDecimal(ApplicationObject appObject, string fieldName, object value, int decimalPlaces)
		{
			if (value != null)
			{
				Decimal val = Decimal.Round(Convert.ToDecimal(value), decimalPlaces);

				SetField(appObject, fieldName, value);
			}
		}

		/// <summary>
		/// Determines if the field in the transaction has the field defined and the field
		/// has a value set to it.
		/// </summary>
		/// <param name="transaction">The transaction.</param>
		/// <param name="fieldName">The field name.</param>
		/// <returns></returns>
		private bool CanFieldBeSet(Transaction transaction, string fieldName)
		{
			// If the transaction has the field defined and the field is set.
			bool result = transaction.HasField(fieldName) && transaction.IsFieldSet(fieldName);
			if (result)
			{
				// Get the value and check the value isn't an arraylist (which should never be true
				// when working in connector transforms).
				object val = transaction.GetValue(fieldName);
				result = !(val is ArrayList);
			}

			return result;
		}

		/// <summary>
		/// This function sets any user-defined values from the transaction to the Application object.
		/// </summary>
		/// <param name="appObject">The application object.</param>
		/// <param name="transaction">The transaction.</param>
		public void SetUserDefinedFields(ApplicationObject appObject, Transaction transaction)
		{
			// Logic to be implemented.
		}

		/// <summary>
		/// A simple helper function that provides a mechanism to determine if a customer
		/// reference exists or not.
		/// </summary>
		/// <param name="reference"></param>
		/// <returns></returns>
		public bool IsExistingCustomer(string reference)
		{
			return false;
		}

		/// <summary>
		/// Provides a means to log an error back onto the audit report.
		/// </summary>
		/// <param name="transaction"></param>
		/// <param name="ex"></param>
		/// <param name="errorMsg"></param>
		public void LogError(Transaction transaction, Exception ex, string errorMsg)
		{
			// Firstly construct the error message.
			string message = string.Empty;
			int errCode = -1;
			if (ex != null)
			{
				if (ex is Exception)
				{
					// Get any specific error message from the application...this may involve cast the exception
					// object to the necessary type.
					string applicationSpecificMsg = "";

					message = string.Format("{0} {1} - {2}", errorMsg, errCode, applicationSpecificMsg);
				}
				else
				{
					message = string.Format("{0} {1}", errorMsg, ex.Message);
				}
			}
			else
			{
				message = errorMsg;
			}

			// Now log the error.
			AuditController.OnError(_transformId, new List<Realisable.Log.ErrorEntry>()
			{
				new Realisable.Log.ErrorEntry()
				{
					ErrorReason = message,
					ResultCode = ex.GetType().Name
				}
			}, transaction);
		}
	}
}
