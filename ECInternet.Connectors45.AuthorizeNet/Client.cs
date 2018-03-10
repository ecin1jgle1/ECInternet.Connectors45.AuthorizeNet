using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Realisable.Utils.DTO;

using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;

namespace ECInternet.Connectors45.Authorize_Net
{
	public class Client
	{
		public string _username = string.Empty;
		public string _password = string.Empty;

		public Client(SystemConnectorDTO systemConnector)
		{
			_username = systemConnector.UserId;
			_password = systemConnector.Password;
		}


		/// <summary>
		/// Capture a Transaction Previously Submitted Via CaptureOnly
		/// </summary>
		/// <param name="pApiLoginID">Your ApiLoginID</param>
		/// <param name="pApiTransactionKey">Your ApiTransactionKey</param>
		/// <param name="pTransactionAmount">The amount submitted with CaptureOnly</param>
		/// <param name="pTransactionID">The TransactionID of the previous CaptureOnly operation</param>
		public static Capture Capture(String pApiLoginID, String pApiTransactionKey, decimal pTransactionAmount, string pTransactionID, bool pSandbox = true)
		{
			Console.WriteLine("Capture Previously Authorized Amount");

			ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = pSandbox ? AuthorizeNet.Environment.SANDBOX : AuthorizeNet.Environment.PRODUCTION;

			// define the merchant information (authentication / transaction id)
			ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
			{
				name = pApiLoginID,
				ItemElementName = ItemChoiceType.transactionKey,
				Item = pApiTransactionKey
			};


			var transactionRequest = new transactionRequestType
			{
				transactionType = transactionTypeEnum.priorAuthCaptureTransaction.ToString(),    // capture prior only
				amount = pTransactionAmount,
				refTransId = pTransactionID
			};

			var request = new createTransactionRequest { transactionRequest = transactionRequest };

			// instantiate the controller that will call the service
			var controller = new createTransactionController(request);
			controller.Execute();

			// get the response from the service (errors contained if any)
			var response = controller.GetApiResponse();

			Capture capture = null;

			// validate response
			if (response != null)
			{
				capture = new Capture();

				if (response.messages.resultCode == messageTypeEnum.Ok)
				{
					if (response.transactionResponse.messages != null)
					{
						capture.TransactionID = response.transactionResponse.transId;
						capture.ResponseCode = response.transactionResponse.responseCode;
						capture.MessageCode = response.transactionResponse.messages[0].code;
						capture.Description = response.transactionResponse.messages[0].description;
						capture.AuthCode = response.transactionResponse.authCode;
					}
					else
					{
						if (response.transactionResponse.errors != null)
						{
							capture.ErrorCode = response.transactionResponse.errors[0].errorCode;
							capture.ErrorMessage = response.transactionResponse.errors[0].errorText;
						}
					}
				}
				else
				{
					if (response.transactionResponse != null && response.transactionResponse.errors != null)
					{
						capture.ErrorCode = response.transactionResponse.errors[0].errorCode;
						capture.ErrorMessage = response.transactionResponse.errors[0].errorText;
					}
					else
					{
						capture.ErrorCode = response.messages.message[0].code;
						capture.ErrorMessage = response.messages.message[0].text;
					}
				}
			}

			return capture;
		}
	}
}
