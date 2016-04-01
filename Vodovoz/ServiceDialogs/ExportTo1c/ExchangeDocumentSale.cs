using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml.Linq;
using Vodovoz.ExportTo1c;

namespace Vodovoz
{
	public class ExchangeDocumentSale:ExchangeObject
	{				
		public override string Type
		{
			get
			{
				return "ДокументСсылка." + DocumentType;
			}
		}

		public override string RuleName
		{
			get
			{
				return DocumentType;
			}
		}

		public string DocumentType{ get; set; }

		public ExportComissionNode Comission{ get; set; }

		public ExportReferenceNode Reference{ get; set;}

		public List<ExportPropertyNode> Properties{ get;}

		public List<ExportTableNode> Tables{ get; set; }

		public ExchangeDocumentSale()
		{
			Properties = new List<ExportPropertyNode>();
			Tables = new List<ExportTableNode>();
			Comission = new ExportComissionNode();
		}			
		public override XElement ToXml()
		{
			var xml = new XElement("Объект",
				new XAttribute("Нпп", Id),
				new XAttribute("Тип", Type),
				new XAttribute("ИмяПравила", RuleName),
				Reference.ToXml()
			);
			xml.Add(Comission.ToXml());
			xml.Add(Tables[0].ToXml());
			Properties.ForEach(prop => xml.Add(prop.ToXml()));
			xml.Add(Tables[1].ToXml());
			//Tables.ForEach(table=>xml.Add(table.ToXml()));
			return xml;
		}
	}
}

