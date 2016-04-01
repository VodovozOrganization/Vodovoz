using System;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Vodovoz
{	
	public class ExchangeCatalogueObject:ExchangeObject
	{
		public override string Type
		{
			get
			{
				return "СправочникСсылка." + CatalogueType;
			}
		}

		public override string RuleName
		{
			get
			{
				return CatalogueType;
			}
		}

		public string CatalogueType{ get; set;}

		public ExportReferenceNode Reference{ get; set;}

		public List<ExportPropertyNode> Properties{ get; set;}

		public ExchangeCatalogueObject()
		{
			Properties = new List<ExportPropertyNode>();
		}

		public override XElement ToXml()
		{
			var xml = new XElement("Объект",
				          new XAttribute("Нпп", Id),
				          new XAttribute("Тип", Type),
				          new XAttribute("ИмяПравила", RuleName),
				          Reference.ToXml()
			          );
			Properties.ForEach(prop => xml.Add(prop.ToXml()));
			return xml;
		}
	}
}

