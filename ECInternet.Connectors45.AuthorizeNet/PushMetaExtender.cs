using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Realisable.Connectors;
using Realisable.Utils;
using Realisable.Utils.DTO;

namespace ECInternet.Connectors45.Authorize_Net
{
	/// <summary>
	/// This class demonstrates how to implement the MetaExtender for a Connector.
	/// </summary>
	/// <remarks>
	/// Connectors must implement ICustomMetaExtender.
	/// </remarks>
	public class PushMetaExtender : ICustomMetaExtender
	{
		private SystemConnectorDTO _connectorSettings;
		private MetaDataDocument _metaDocument;
		private string _currentImportId;

		private const string CUSTOM_RECORD_ATTRIBUTE = "customAttrib";

		private const string FLD_CUSTOM_1 = "CustomAttrib1";
		private const string FLD_CUSTOM_2 = "CustomAttrib2";

		private Dictionary<string, bool> _updateFlag = new Dictionary<string, bool>();

		public PushMetaExtender()
		{
			// This is optional and it depends how dynamic you're metadata is!
			//
			// If the meta data is static it may be easier to create an Xml document (in the form of the embedded resource)
			// and simply pass the stream from the embedded resource to the constructor of the MetaDataDocument class.
			//
			// If you are dealing with dynamic data see the set_SystemId and SetImportId properties.
			//
			// If there is a mixture of static and dynamic data, employing a static 'base' MetaDataDocument containing
			// just the base fields/record types, combining with the querying the application's metadata for the dynamic 
			// component may be the most efficient/quickest.
			//
			// If you are dealing with purely dynamic data, constructing your own MetaDataDocument may be the only choice.
			// 
			// ResourceHelper.GetEmbeddedResource is a helper method defined in Realisable.Utils assembly.
			_metaDocument = new MetaDataDocument(ResourceHelper.GetEmbeddedResource(Constants.PUSH_META_DOCUMENT, GetType().Assembly));
		}

		#region ICustomMetaExtender Members

		/// <summary>
		/// Returns a 48 x 48 pixel bitmap.
		/// The bitmap is used as the icon of the connector on the design screen.
		/// </summary>
		public Bitmap ConnectorBitmap
		{
			get
			{
				return new Bitmap(ResourceHelper.GetEmbeddedResource(Constants.PUSH_CONNECTOR_ICON, GetType().Assembly));
			}
		}

		/// <summary>
		/// Gets/sets the SystemConnectorDTO object of the extender.
		/// This property is set when Select System drop down is selected on the Connector User Interface.
		/// </summary>
		public SystemConnectorDTO ConnectorSettings
		{
			get
			{
				return _connectorSettings;
			}
			set
			{
				// Upon setting the system id, you may want to go interogate the
				// corresponding system/application to query & retrieve the application's metadata.

				// If the SystemId value is different to the currently stored value,
				// the SystemConnector is being changed (or initiated if SystemId is empty/null).
				if (_connectorSettings == null || _connectorSettings.SystemId != value.SystemId)
				{
					// Clean up any existing resources if they are open.
					_updateFlag = new Dictionary<string, bool>();

					// Create the MetaDataDocument dynamically.
					// See class constructor when to create this object.
					_metaDocument = new MetaDataDocument(ResourceHelper.GetEmbeddedResource(Constants.PUSH_META_DOCUMENT, GetType().Assembly));
				}

				// Open the connection to the resource/application.
				// <do work here>

				// Set the local instance.
				_connectorSettings = value;
			}
		}

		/// <summary>
		/// Returns any Custom Attributes the FieldAttributes objects may have.
		/// The returned list will auto-populate onto the configuration file,
		/// and will be accessible on the FieldDefinition object when processing.
		/// </summary>
		public List<string> CustomFieldAttributes
		{
			get
			{
				List<string> result = new List<string>();

				result.Add(FLD_CUSTOM_1);
				result.Add(FLD_CUSTOM_2);

				return result;
			}
		}

		/// <summary>
		/// Returns any Custom Attributes the RecordAttributes objects may have.
		/// The returned list will auto-populate onto the configuration file,
		/// and will be accessible on the RecordDefinition object when processing.
		/// </summary>
		public List<string> CustomRecordAttributes
		{
			get
			{
				List<string> result = new List<string>();

				result.Add(CUSTOM_RECORD_ATTRIBUTE);

				return result;
			}
		}

		/// <summary>
		/// Gets/sets the import id/type. This function is called when the Import Type dropdown
		/// control is selected on the Connector User Interface.
		/// </summary>
		/// <remarks>
		/// Use this function to cache and prettify (translate the field ids to the display name) the static records/fields
		/// and retrieve any dynamic data for the relevant import.
		/// 
		/// Whilst this could be done when the initial SystemId is set, on a large application with alot of import types,
		/// this may take a long time, so only do with necessary.
		/// </remarks>
		public string ImportType
		{
			get
			{
				return _currentImportId;
			}
			set
			{
				_currentImportId = value;

				bool logonOpened = false;

				try
				{
					// Iterate through RecordAttributes
					foreach (RecordAttributes recordAttribs in _metaDocument[_currentImportId])
					{
						string id = _currentImportId + "-" + recordAttribs.Id;

						// Use a dictionary to denote import types already processed
						// ...i.e. do not re-process imports already cached/processed.
						if (!_updateFlag.ContainsKey(id) || !_updateFlag[id])
						{
							// It may be neccessary to go and connect to the external system at this point. E.g.
							if (!logonOpened)
							{
								OpenConnection();
								logonOpened = true;
							}

							// This method updates the description of each field by placing a space between each word.
							// For each of the RecordAttributes in the import.
							List<FieldAttributes> fields = recordAttribs.Fields;

							// For each of the fields.
							foreach (FieldAttributes fldAttribs in fields)
							{
								// If the string is empty, take the Id and make it friendly by Capitilising and adding spaces.
								if (string.IsNullOrEmpty(fldAttribs.Description))
								{
									StringBuilder sb = new StringBuilder();

									char last = char.MinValue;
									foreach (char c in fldAttribs.Id)
									{
										if (char.IsLower(last) && char.IsUpper(c))
										{
											sb.Append(' ');
										}

										sb.Append(c);
										last = c;
									}

									// Update the description.
									fldAttribs.Description = sb.ToString();
								}
							}

							// Both record and field attributes are simply stored as a <string, string> dictionary object.
							IDictionary<string, string> attribs = recordAttribs.Attributes;

							// If the record attributes contains a certain attribute then do something!
							if (attribs.ContainsKey(CUSTOM_RECORD_ATTRIBUTE))
							{
								// At this point go and query the application to retrieve any dynamic metadata.
								// This following section adds a user-defined field.

								// Declare the object.
								FieldAttributes userDefinedField = new FieldAttributes();
								userDefinedField.Id = "FieldId";
								userDefinedField.Description = "Field Description";

								// Add any attributes to the field (optional).
								// The field attributes should correspond to the List returned in the CustomFieldAttributes property.
								userDefinedField.Attributes[FLD_CUSTOM_1] = "value1";
								userDefinedField.Attributes[FLD_CUSTOM_2] = "value2";

								// Add the field into the field list.
								fields.Add(userDefinedField);
							}

							// Finally update the internal cache object, so you don't have to repeat expensive operations.
							_updateFlag.Add(id, true);
						}
					}
				}
				finally
				{
					if (logonOpened)
					{
						CloseConnection();
					}
				}
			}
		}

		/// <summary>
		/// Returns the currently open MetaDataDocument object.
		/// This property is called only after the SystemId property is set,
		/// and therefore this property should never return null!
		/// </summary>
		public MetaDataDocument MetaDataDocument
		{
			get
			{
				return _metaDocument;
			}
		}

		/// <summary>
		/// Called immediately after SystemId is set. This property should return only the SystemImports the current
		/// instance of the application supports. The returned list should either be all or a subset of the total possible
		/// SystemImport objects.
		/// </summary>
		/// <remarks>
		/// Use this to filter/create the SystemImport list in the MetaDataDocument when an application has different functionality
		/// For example Basic/Professional/Enterprise editions.
		/// 
		/// Upon setting the SystemId you may query the application and retrieve the relevant meta data.
		/// You would then filter the SystemImport objects within the MetaDataDocument and return the resulting list.
		/// </remarks>
		public List<SystemImport> PermittedImports
		{
			get
			{
				if (_connectorSettings == null)
				{
					throw new InvalidOperationException("SystemConnector has not been set.");
				}

				// Do some work to filter the MetaDataDocument (if not done already).
				// <Do work here>

				// Depending on how localised you need to display the system/record/field values on the Connector transforms
				// either name property on the MetaDataDocument or use resources to convert the id to description.

				// Return the resulting list.
				return new List<SystemImport>(_metaDocument);
			}
		}

		/// <summary>
		/// Return the formatted name of the connector.
		/// This will be displayed within the designer.
		/// </summary>
		/// <param name="language">
		/// The language identifier of the currently logged-on user.
		/// Reserved for future use, presently this will always be 'en'.
		/// </param>
		/// <returns>Connector name.</returns>
		public string PrettifiedConnectorName(string language)
		{
			return Constants.PUSH_CONNECTOR_NAME;
		}

		#endregion

		/// <summary>
		/// Do something to open a connection to the application, in order to query the applications meta data.
		/// </summary>
		private void OpenConnection()
		{
		}

		/// <summary>
		/// Do something to close the connection to the application.
		/// </summary>
		private void CloseConnection()
		{
		}
	}
}
