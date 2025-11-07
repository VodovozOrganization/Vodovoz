using System.Xml.Linq;
using Vodovoz.Tools;

namespace ExportTo1c.Library.ExportNodes
{
	/// <summary>
	/// Значение
	/// </summary>
	public class PropertyValueNode : IXmlConvertable
	{
		public string Value { get; set; }

		public PropertyValueNode(string value)
		{
			Value = value;
		}

		public XElement ToXml()
		{
			XElement xml = new XElement("Значение");
			xml.Value = Value ?? "";
			return xml;
		}
	}
}
