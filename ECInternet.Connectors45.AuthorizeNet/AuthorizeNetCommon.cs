using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AuthorizeNet;

namespace ECInternet.Connectors45.Authorize_Net
{
	public class AuthorizeNetCommon
	{
		public IGateway OpenGateway()
		{
			//we used the form builder so we can now just load it up
			//using the form reader
			var login = ConfigurationManager.AppSettings["ApiLogin"];
			var transactionKey = ConfigurationManager.AppSettings["TransactionKey"];

			//this is set to test mode - change as needed.
			var gate = new Gateway(login, transactionKey, true);
			return gate;
		}


	}
}
