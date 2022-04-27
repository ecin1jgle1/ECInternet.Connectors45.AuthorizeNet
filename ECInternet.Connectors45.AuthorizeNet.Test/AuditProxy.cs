using System.Collections.Generic;
using System.Diagnostics;
using Realisable.Data.Transform;
using Realisable.Log;

namespace ECInternet.Connectors45.Authorize_Net.Test
{
	/// <summary>
	/// Provides an AuditController proxy, so that any errors/warnings are emitted to the console/output.
	/// </summary>
	internal class AuditProxy : ITransformAuditController
	{
		#region ITransformAuditController Members

		public void OnError(string transformId, List<ErrorEntry> errors, Transaction transaction)
		{
			foreach (ErrorEntry entry in errors)
			{
				WriteLogEntries(AuditEventType.eEventError, entry);
			}
		}

		public void OnWarning(string transformId, List<ErrorEntry> warnings, Transaction transaction)
		{
			foreach (ErrorEntry entry in warnings)
			{
				WriteLogEntries(AuditEventType.eEventError, entry);
			}
		}

		public void OnProcess(string transformId, Transaction transaction)
		{
		}

		public void OnInsert(string transformId, Transaction transaction)
		{
		}

		public void OnUpdate(string transformId, Transaction transaction)
		{
		}

		public void OnDelete(string transformId, Transaction transaction)
		{
		}

		public void OnComplete(string transformId)
		{
		}

		public int GetEventCount(string transformId, string recordId, AuditEventType eventType)
		{
			return 0;
		}

		public void WriteAuditLog(string transformId, List<ErrorEntry> entries)
		{
		}

		public void WriteAuditLog(string transformId, List<ErrorEntry> entries, Transaction transactionBeingAudited)
		{
		}

		public void WriteAuditLog(ErrorEntry logEntry)
		{
		}

		public void WriteAuditLog(string transformId, ErrorEntry logEntry)
		{
		}

		public void WriteAuditLog(string transformId, ErrorEntry logEntry, Transaction transactionBeingAudited)
		{
		}

		#endregion

		private void WriteLogEntries(AuditEventType eventType, ErrorEntry logEntry)
		{
			string message = string.Format("{0} - {1} - {2} - {3}",
				new object[]
				{
					eventType.ToString(), logEntry.ResultCode, logEntry.Source, logEntry.ErrorReason
				});
			Debug.WriteLine(message);
		}

		public void WriteTrace(string transformId, TraceType traceType, Realisable.Data.Transform.TraceLevel level, string traceName, string data)
		{
			throw new System.NotImplementedException();
		}
	}
}
