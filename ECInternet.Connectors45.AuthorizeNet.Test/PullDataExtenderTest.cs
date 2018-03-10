using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Realisable.Connectors;

namespace ECInternet.Connectors45.Authorize_Net.Test
{
	[TestClass]
	public class PullDataExtenderTest
	{
		PullMetaExtender _pde = new PullMetaExtender();

		[TestMethod]
		public void PrettifiedConnectorNameTest()
		{
			string connectorName = _pde.PrettifiedConnectorName("");
		}

		[TestMethod]
		public void ConnectorBitmapTest()
		{
			Bitmap bitmap = _pde.ConnectorBitmap;
			Assert.IsNotNull(bitmap);
		}

		[TestMethod]
		public void CustomRecordAttributesTest()
		{
			List<string> customerRecordAttributes = _pde.CustomRecordAttributes;
		}

		[TestMethod]
		public void CustomFieldAttributesTest()
		{
			List<string> customFieldAttributes = _pde.CustomFieldAttributes;
		}

		[TestMethod]
		public void GetReaderOptionsTest()
		{
			IList<ReaderOption> readerOptions = _pde.GetReaderOptions("");
		}
	}
}
