﻿using System.Collections.Generic;
using System.Xml.Linq;

namespace ExportTo1c.Library.ExportNodes
{
	/// <summary>
	/// Счёт-фактура
	/// </summary>
	public class InvoiceDocumentNode : ObjectNode
	{
		public override string Type
		{
			get { return Common1cTypes.InvoiceDocument; }
		}

		public override string RuleName
		{
			get { return "СчетФактураВыданный"; }
		}

		public List<PropertyNode> Properties { get; set; }
		public ReferenceNode Reference { get; set; }

		public InvoiceDocumentNode()
		{
			Properties = new List<PropertyNode>();
		}

		#region implemented abstract members of ExchangeObject

		public override XElement ToXml()
		{
			var xml = new XElement("Объект",
				new XAttribute("Нпп", Id),
				new XAttribute("Тип", Type),
				new XAttribute("ИмяПравила", RuleName)
			);
			xml.Add(Reference.ToXml());
			Properties.ForEach(prop => xml.Add(prop.ToXml()));
			return xml;
		}

		#endregion
	}
}
