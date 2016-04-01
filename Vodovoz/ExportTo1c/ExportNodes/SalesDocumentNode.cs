using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml.Linq;
using Vodovoz.ExportTo1c;

namespace Vodovoz.ExportTo1c
{
	public class SalesDocumentNode:ObjectNode
	{				
		public override string Type
		{
			get
			{
				return Common1cTypes.SalesDocument;
			}
		}

		public override string RuleName
		{
			get
			{
				return "РеализацияТоваровУслуг";
			}
		}
			
		public ComissionNode Comission{ get; set; }

		public ReferenceNode Reference{ get; set;}

		public List<PropertyNode> Properties{ get;}

		public List<TableNode> Tables{ get; set; }

		public SalesDocumentNode()
		{
			Properties = new List<PropertyNode>();
			Tables = new List<TableNode>();
			Comission = new ComissionNode();
		}			
		public override XElement ToXml()
		{
			var xml = new XElement("Объект",
				new XAttribute("Нпп", Id),
				new XAttribute("Тип", Type),
				new XAttribute("ИмяПравила", RuleName)
			);
			xml.Add(Reference.ToXml());
			xml.Add(Comission.ToXml());
			xml.Add(Tables[0].ToXml());
			Properties.ForEach(prop => xml.Add(prop.ToXml()));
			xml.Add(Tables[1].ToXml());
			//Tables.ForEach(table=>xml.Add(table.ToXml()));
			return xml;
		}
	}
}

