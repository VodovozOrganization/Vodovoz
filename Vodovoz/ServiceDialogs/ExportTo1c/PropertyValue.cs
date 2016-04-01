using System;
using System.Xml.Linq;

namespace Vodovoz
{
	public class PropertyValue : IXmlConvertable
	{
		public string Value{get;set;}

		public PropertyValue(string value)
		{
			this.Value = value;	
		}

		public XElement ToXml()
		{
			XElement xml = new XElement("Значение");
			xml.Value = Value;
			return xml;
		}
	}
}

