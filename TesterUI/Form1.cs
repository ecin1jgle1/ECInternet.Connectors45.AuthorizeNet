using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace TesterUI
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void buttonAuthorize_Click(object sender, EventArgs e)
		{
			//pull from the store

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

			//set the amount - you can also set this from the page itself
			//you have to have a field named "x_amount"
			apiRequest.Queue(ApiFields.Amount, "1.23");
			//You can set your solution ID here.
			apiRequest.SolutionID = "AAA100302";

			//send to Auth.NET
			var response = gate.Send(apiRequest);

			string authCode;
			string transactionID;
			string orderMessage;
			if (response.Approved)
			{
				authCode = response.AuthorizationCode;
				transactionID = response.TransactionID;
				orderMessage = string.Format("Thank you! Order approved: {0}", response.AuthorizationCode);
			}
			else
			{
				//error... oops. Reload the page
				orderMessage = response.Message;
			}

		}
	}
}
