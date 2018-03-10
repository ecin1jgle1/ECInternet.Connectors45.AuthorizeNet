using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ECInternet.Connectors45.Authorize_Net.Test
{
	[TestClass]
	public class PushMetaExtenderTest
	{
		PushMetaExtender _pme = new PushMetaExtender();

		[TestMethod]
		public void PrettifiedConnectorNameTest()
		{
			string connectorName = _pme.PrettifiedConnectorName("");
		}

		[TestMethod]
		public void ConnectorBitmapTest()
		{
			Bitmap bitmap = _pme.ConnectorBitmap;
			Assert.IsNotNull(bitmap);
		}

		[TestMethod]
		public void CustomRecordAttributesTest()
		{
			List<string> customerRecordAttributes = _pme.CustomRecordAttributes;
		}

		[TestMethod]
		public void CustomFieldAttributesTest()
		{
			List<string> customFieldAttributes = _pme.CustomFieldAttributes;
		}
	}
}
