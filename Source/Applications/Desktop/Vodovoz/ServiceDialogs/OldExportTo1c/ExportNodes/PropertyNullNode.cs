using System;
using System.Xml.Linq;

namespace Vodovoz.OldExportTo1c
{
	public class PropertyNullNode : IXmlConvertable
	{
		public PropertyNullNode()
		{
		}			

		public System.Xml.Linq.XElement ToXml()
		{
			XElement xml = new XElement("Пусто");
			return xml;
		}			
	}
}

