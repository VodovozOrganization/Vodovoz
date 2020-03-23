using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Vodovoz.OldExportTo1c
{	
	public class TableNode : IXmlConvertable
	{
		public string Name{ get; set;}

		public List<TableRecordNode> Records{ get; set; }

		public TableNode()
		{
			Records = new List<TableRecordNode>();
		}			

		public XElement ToXml()
		{
			var xml = new XElement("ТабличнаяЧасть",
				new XAttribute("Имя",Name)
			);
			Records.ForEach(record=>xml.Add(record.ToXml()));
			return xml;
		}
	}
}

