using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml.Linq;
using Vodovoz.ExportTo1c;
using Vodovoz.ServiceDialogs.ExportTo1c;

namespace Vodovoz.ExportTo1c
{
	public class RetailDocumentNode:ObjectNode
	{				
		public override string Type
		{
			get
			{
				return Common1cTypes.RetailDocument;
			}
		}

		public override string RuleName
		{
			get
			{
				return "ОтчетОРозничныхПродажах";
			}
		}

		public ReferenceNode Reference{ get; set;}

		public List<PropertyNode> Properties { get; private set;}

		public List<TableNode> Tables{ get; set; }

		public RetailDocumentNode()
		{
			Properties = new List<PropertyNode>();
			Tables = new List<TableNode>();
		}			
		public override XElement ToXml()
		{
			var xml = new XElement("Объект",
				new XAttribute("Нпп", Id),
				new XAttribute("Тип", Type),
				new XAttribute("ИмяПравила", RuleName)
			);
			xml.Add(Reference.ToXml());
			xml.Add(Tables[0].ToXml());
			Properties.ForEach(prop => xml.Add(prop.ToXml()));
			xml.Add(Tables[1].ToXml());
			return xml;
		}
	}
}

