using System.Xml;

namespace ECInternet.Connectors45.Authorize_Net
{
	/// <summary>
	/// Provides a structure for recording the state of a reader.
	/// </summary>
	internal class ReaderPositionState
	{
		public XmlNodeList CurrentNodes { get; set; }
		public XmlNode CurrentNode { get; set; }
		public string CurrentId { get; set; }
		public int CurrentPosition { get; set; }
	}
}
