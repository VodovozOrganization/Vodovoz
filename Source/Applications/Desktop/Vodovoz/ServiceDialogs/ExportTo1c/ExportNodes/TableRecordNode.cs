using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace Vodovoz.ExportTo1c
{	
	public class TableRecordNode:IXmlConvertable
	{		
		public List<PropertyNode> Properties{ get; set;}
		public TableRecordNode()
		{
			Properties = new List<PropertyNode>();
		}			

		public XElement ToXml()
		{
			var xml = new XElement("Запись");
			Properties.ForEach(prop => xml.Add(prop.ToXml()));
			return xml;
		}			
	}
}

