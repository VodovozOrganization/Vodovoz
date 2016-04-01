using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace Vodovoz
{	
	public class ExportTableRecordNode:IXmlConvertable
	{		
		public List<ExportPropertyNode> Properties{ get; set;}
		public ExportTableRecordNode()
		{
			Properties = new List<ExportPropertyNode>();
		}			

		public XElement ToXml()
		{
			var xml = new XElement("Запись");
			Properties.ForEach(prop => xml.Add(prop.ToXml()));
			return xml;
		}			
	}
}

