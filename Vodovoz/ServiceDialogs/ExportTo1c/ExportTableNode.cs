using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml.Linq;
using Vodovoz.ExportTo1c;

namespace Vodovoz
{	
	public class ExportTableNode : IXmlConvertable
	{
		public string Name{ get; set;}

		public List<ExportTableRecordNode> Records{ get; set; }

		public ExportTableNode()
		{
			Records = new List<ExportTableRecordNode>();
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

