﻿using System.Collections.Generic;
using System.Xml.Linq;

namespace ExportTo1c.Library.ExportNodes
{
	public class CatalogObjectNode : ObjectNode
	{
		public override string Type
		{
			get { return "СправочникСсылка." + CatalogueType; }
		}

		public override string RuleName
		{
			get { return CatalogueType; }
		}

		public string CatalogueType { get; set; }

		public ReferenceNode Reference { get; set; }

		public List<PropertyNode> Properties { get; set; }

		public CatalogObjectNode()
		{
			Properties = new List<PropertyNode>();
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
