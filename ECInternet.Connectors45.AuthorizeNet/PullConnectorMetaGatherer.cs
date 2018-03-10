using System.IO;
using System.Xml;
using Realisable.Connectors;
using Realisable.Resources;
using Realisable.Utils;

namespace ECInternet.Connectors45.Authorize_Net
{
	/// <summary>
	/// This generates a MetaDataDocument dynamically.
	/// </summary>
	/// <remarks>
	/// This is a mock demonstration, but in a real world scenario the metadata would be 
	/// derived from a more dynamic source than a static XmlDocument...i.e. a webservice.
	/// </remarks>
	internal class PullConnectorMetaGatherer
	{
		private const string META_XML_DOCUMENT = "PullMetaConnectorSource.xml";

		/// <summary>
		/// Generates a MetaDataDocument.
		/// </summary>
		/// <returns>The constructed MetaDataDocument.</returns>
		public static MetaDataDocument RetrieveMetaDocument()
		{
			// Create an empty document object.
			MetaDataDocument result = new MetaDataDocument();

			XmlDocument document = OpenMetaDocument();

			// Each of the DataType nodes represent a SystemImport object.
			foreach (XmlNode dataTypeNode in document.SelectNodes("/MetaData/DataType"))
			{
				// Create the SystemImport object and add it to resultant object.
				SystemImport import = new SystemImport(dataTypeNode.Attributes["name"].Value, dataTypeNode.Attributes["name"].Value, eERPUpdateOperation.eAll);
				result.AddImport(import);

				// Get the Record node
				XmlNode recordAttributeNode = dataTypeNode.SelectSingleNode("Record");

				// Now call the following method to add the Records recursively (due to the recursive Xml structure of PullMetaConnectorSource.xml).
				AddRecordAttributeRecursive(import, recordAttributeNode, string.Empty);
			}

			return result;
		}

		/// <summary>
		/// Recursively adds RecordAttributes to a SystemImport object.
		/// </summary>
		/// <param name="import">The SystemImport object.</param>
		/// <param name="recordAttributeNode">The XmlNode representing the RecordAttributes object to be created.</param>
		/// <param name="parentId">The id of the parent RecordAttributes object.</param>
		private static void AddRecordAttributeRecursive(SystemImport import, XmlNode recordAttributeNode, string parentId)
		{
			// Create the RecordAttributes object.
			RecordAttributes recordAttribute = new RecordAttributes
			{
				Id = recordAttributeNode.Attributes["name"].Value,
				ParentId = parentId,
				Mandatory = true, 
				Description = recordAttributeNode.Attributes["name"].Value
			};

			// Add it to the SystemImport object.
			import[recordAttribute.Id] = recordAttribute;

			// Add the fields.
			AddFields(recordAttribute, recordAttributeNode);

			// Now recursively add each child Record XmlNode.
			foreach (XmlNode childRecordAttribute in recordAttributeNode.SelectNodes("Record"))
			{
				AddRecordAttributeRecursive(import, childRecordAttribute, recordAttribute.Id);
			}
		}

		/// <summary>
		/// Adds fields to a RecordAttribute object.
		/// </summary>
		/// <param name="recordAttribute">The RecordAttribute attribute to add fields to.</param>
		/// <param name="recordAttributeNode">The XmlNode containing the fields.</param>
		private static void AddFields(RecordAttributes recordAttribute, XmlNode recordAttributeNode)
		{
			foreach (XmlNode fieldNode in recordAttributeNode.SelectNodes("Field"))
			{
				FieldAttributes field = new FieldAttributes
				{
					Id = fieldNode.Attributes["name"].Value,
					Description = fieldNode.Attributes["name"].Value,
					IsWriteBack = false,
					FieldType = TranslateFieldType(fieldNode.Attributes["type"].Value)
				};

				recordAttribute[field.Id] = field;
			}
		}

		/// <summary>
		/// Translates the 'type' attribute on a field to a Realisable.Resources.IntManFieldTypeEnum value.
		/// </summary>
		/// <param name="fieldType">The string to be translated.</param>
		/// <returns>The translated value.</returns>
		private static IntManFieldTypeEnum TranslateFieldType(string fieldType)
		{
			switch (fieldType)
			{
				case "string":
					return IntManFieldTypeEnum.eString;
				case "decimal":
					return IntManFieldTypeEnum.eDecimal;
				case "bool":
					return IntManFieldTypeEnum.eBoolean;
				case "integer":
					return IntManFieldTypeEnum.eLongInt;
				case "date":
					return IntManFieldTypeEnum.eDate;
				default:
					return IntManFieldTypeEnum.eString;
			}
		}

		/// <summary>
		/// Opens the embedded XmlDocument.
		/// </summary>
		/// <returns>The XmlDocument.</returns>
		private static XmlDocument OpenMetaDocument()
		{
			XmlDocument document = new XmlDocument();

			Stream stream = ResourceHelper.GetEmbeddedResource(META_XML_DOCUMENT, typeof(PullConnectorMetaGatherer).Assembly);

			document.Load(stream);

			return document;
		}
	}
}
