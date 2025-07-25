using System.Xml.Linq;
using Vodovoz.Tools;

namespace ExportTo1c.Library.ExportNodes
{
	public class PropertyNullNode : IXmlConvertable
	{
		public PropertyNullNode()
		{
		}

		public XElement ToXml()
		{
			XElement xml = new XElement("Пусто");
			return xml;
		}
	}
}
