using System;
using System.Xml.Linq;

namespace Vodovoz.ExportTo1c
{
	public class PropertyValueNode : IXmlConvertable
	{
		public string Value{get;set;}

		public PropertyValueNode(string value)
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

