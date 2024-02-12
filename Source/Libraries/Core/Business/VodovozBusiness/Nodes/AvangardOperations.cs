using System;
using System.Xml.Serialization;

namespace Vodovoz.Nodes
{
	[Serializable]
	[XmlRoot(ElementName = "operations")]
	public class AvangardOperations
	{
		[XmlElement("operation")]
		public AvangardOperation[] Operations { get; set; }
	}
}
