using System.Xml.Linq;
using Vodovoz.Tools;

namespace ExportTo1c.Library.ExportNodes
{
	/// <summary>
	/// Пустое значение
	/// </summary>
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
