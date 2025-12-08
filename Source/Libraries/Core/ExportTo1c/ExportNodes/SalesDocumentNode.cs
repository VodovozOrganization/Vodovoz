using System.Collections.Generic;
using System.Xml.Linq;
using Vodovoz.EntityRepositories.Orders;

namespace ExportTo1c.Library.ExportNodes
{
	/// <summary>
	/// Реализация товаров
	/// </summary>
	public class SalesDocumentNode : ObjectNode
	{
		public override string Type
		{
			get { return Common1cTypes.SalesDocument; }
		}

		public override string RuleName
		{
			get { return "РеализацияТоваровУслуг"; }
		}

		public ComissionNode Comission { get; set; }

		public ReferenceNode Reference { get; set; }

		public List<PropertyNode> Properties { get; private set; }

		public List<TableNode> Tables { get; set; }
		public Export1cMode ExportMode { get; set; }

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

			if(ExportMode != Export1cMode.ComplexAutomation)
			{
				xml.Add(Comission.ToXml());
			}

			xml.Add(Tables[0].ToXml());
			Properties.ForEach(prop => xml.Add(prop.ToXml()));

			if(Tables.Count > 1)
			{
				xml.Add(Tables[1].ToXml());
			}

			return xml;
		}
	}
}
