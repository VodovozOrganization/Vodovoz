using System;
using System.Xml.Linq;

namespace Vodovoz
{
	public class PropertyNull : IXmlConvertable
	{
		public PropertyNull()
		{
		}			

		public System.Xml.Linq.XElement ToXml()
		{
			XElement xml = new XElement("Пусто");
			return xml;
		}			
	}
}

