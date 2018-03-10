using System;
using System.Collections.Generic;
using System.Xml;
using Realisable.Data.Transform;
using Realisable.JobDefinition;
using Realisable.Resources;
using Realisable.Utils;
using Realisable.Utils.DTO;

namespace ECInternet.Connectors45.Authorize_Net
{
	public class PullDataInterop : IReader, IDisposable
	{
		private readonly XmlDocument _data = new XmlDocument();
		private readonly Stack<ReaderPositionState> _readerState = new Stack<ReaderPositionState>();

		private string _systemId;
		private bool _testMode;

		private string _readerOption1;
		private string _readerOption2;

		#region IReader Members

		/// <summary>
		/// Called to initialise the Reader object.
		/// </summary>
		/// <param name="transformDef">The transform definition.</param>
		/// <param name="testMode">Indicates true/false if the Reader is being invoked from the designer/preview mode.</param>
		/// <param name="system">The SystemConnectorDTO object containing the settings.</param>
		public void Initialise(TransformDefinition transformDef, bool testMode, SystemConnectorDTO system)
		{
			string importId = transformDef.Options.GetProperty(GlobalResources.SYS_CONNECT_TRAN_TYPE_OPT);

			_systemId = system.SystemId;
			_testMode = testMode;

			GetReaderOptions(transformDef);

			OpenDataDocument(importId);
		}

		/// <summary>
		/// Returns an array of values corresponding to the fieldIds argument.
		/// </summary>
		/// <param name="fieldIds">The fieldIds to return.</param>
		/// <returns>A string array with the corresponding field values.</returns>
		public string[] GetValues(ref string[] fieldIds)
		{
			XmlNode currentNode = _readerState.Peek().CurrentNode;

			string[] result = new string[fieldIds.Length];

			for (int i = 0; i < fieldIds.Length; i++)
			{
				string fieldId = fieldIds[i];

				XmlNode fieldNode = currentNode.SelectSingleNode("Field[@name='" + fieldId + "']");
				if (fieldNode != null)
				{
					result[i] = fieldNode.Attributes["value"].Value;
				}
			}

			return result;
		}

		/// <summary>
		/// Called to move to a child record.
		/// Returns true when the position can be moved to a child. 
		/// </summary>
		/// <param name="recordId">The recordId to move to.</param>
		/// <returns>True/false indicating if operation succeeded.</returns>
		public bool MoveChild(string recordId)
		{
			XmlNode currentNode = _readerState.Peek().CurrentNode;
			XmlNode childNode = currentNode.SelectSingleNode("Records[@name='" + recordId + "']");

			PushReaderState(childNode);

			return childNode != null;
		}

		/// <summary>
		/// Moves to the next record.
		/// Returns true when record position can be advanced and false when there are no more records to be retrieved.
		/// </summary>
		/// <returns>True/false indicating if operation succeeded.</returns>
		public bool MoveNext()
		{
			ReaderPositionState state = _readerState.Peek();

			if (state.CurrentNodes.Count > state.CurrentPosition + 1)
			{
				++state.CurrentPosition;
				state.CurrentNode = state.CurrentNodes[state.CurrentPosition];

				return true;
			}

			return false;
		}

		/// <summary>
		/// Moves the position to the parent.
		/// MoveParent is called irrespective of whether the corresponding MoveChild method returned true or false.
		/// </summary>
		/// <returns>True/false indicating if operation succeeded.</returns>
		public bool MoveParent()
		{
			if (_readerState.Count > 0)
			{
				_readerState.Pop();

				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns the number of records within current node context.
		/// </summary>
		public int RecordCount
		{
			get
			{
				ReaderPositionState currentState = _readerState.Peek();

				if (currentState.CurrentNodes != null)
				{
					return currentState.CurrentNodes.Count;
				}
				else
				{
					return 0;
				}
			}
		}

		/// <summary>
		/// Returns a string indicating the source of the transaction.
		/// The return value can be any string, but it should help to identify the query/record(s) being read.
		/// This property is used whenever there is an exception.
		/// </summary>
		public string Source
		{
			get
			{
				return string.Empty;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
		}

		#endregion

		/// <summary>
		/// Pushes the reader state onto the stack.
		/// </summary>
		/// <param name="node">The node to be pushed onto the stack.</param>
		private void PushReaderState(XmlNode node)
		{
			// If the node is not null then create a new object with the values.
			// Otherwise, if the node is null, simply push an empty object onto the stack
			// since the MoveParent method will be called immediately afterward.
			if (node != null)
			{
				ReaderPositionState state = new ReaderPositionState
				{
					CurrentId = node.Attributes["name"].Value,
					CurrentNodes = node.SelectNodes("Record"),
					CurrentPosition = -1
				};

				_readerState.Push(state);
			}
			else
			{
				_readerState.Push(new ReaderPositionState());
			}
		}

		/// <summary>
		/// Opens the embedded XmlDocument.
		/// </summary>
		/// <returns>The XmlDocument.</returns>
		private void OpenDataDocument(string importId)
		{
			string documentId = string.Empty;

			switch (importId)
			{
				case "Customer":
					documentId = "PullConnectorCustomerData.xml";
					break;

				case "Order":
					documentId = "PullConnectorOrderData.xml";
					break;
			}

			System.IO.Stream stream = ResourceHelper.GetEmbeddedResource(documentId, this.GetType().Assembly);

			_data.Load(stream);

			PushReaderState(_data.SelectSingleNode("/Records"));
		}

		/// <summary>
		/// Demonstrates how to get the ReaderOptions from the Transform Definition.
		/// </summary>
		/// <param name="transformDef">The transformDefintion</param>
		private void GetReaderOptions(TransformDefinition transformDef)
		{
			_readerOption1 = GetReaderOptionValue(transformDef, "Option1");
			_readerOption2 = GetReaderOptionValue(transformDef, "Option2");
		}

		private string GetReaderOptionValue(TransformDefinition transformDef, string property)
		{
			return transformDef.Options.GetProperty(property);
		}
	}
}
