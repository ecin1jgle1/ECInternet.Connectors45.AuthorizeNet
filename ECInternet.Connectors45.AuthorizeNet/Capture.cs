using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuthorizeNet.Api.Contracts.V1;

namespace ECInternet.Connectors45.Authorize_Net
{
	public class Capture
	{
		public string ApiLoginID { get; set; }
		public string ApiTransactionKey { get; set; }
		public decimal TransactionAmount { get; set; }
		public string TransactionID { get; set; }
		public string ResponseCode { get; set; }
		public string MessageCode { get; set; }
		public string Description { get; set; }
		public string AuthCode { get; set; }
		public string ErrorCode { get; set; }
		public string CaptureErrorMessage { get; set; }

		public string ToTraceOutput()
		{
			string trace = string.Empty;

			List<string> list = new List<string>();
			list.Add(string.Format("Transaction Amount:\t[{0:C}]", TransactionAmount));
			list.Add(string.Format("Transaction ID:\t[{0}]", TransactionID));
			list.Add(string.Format("Response Code:\t[{0}]", ResponseCode));
			list.Add(string.Format("Message Code:\t[{0}]", MessageCode));
			list.Add(string.Format("Description:\t[{0}]", Description));
			list.Add(string.Format("Auth Code:\t[{0}]", AuthCode));
			list.Add(string.Format("Error Code:\t[{0}]", ErrorCode));
			list.Add(string.Format("Error Message:\t[{0}]", CaptureErrorMessage));

			trace = string.Join("\n\t", list.ToArray());

			return trace;
		}
	}
}
