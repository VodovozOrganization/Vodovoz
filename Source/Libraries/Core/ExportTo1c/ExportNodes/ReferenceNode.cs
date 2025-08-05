using System.Collections.Generic;
using System.Xml.Linq;
using Vodovoz.Tools;

namespace ExportTo1c.Library.ExportNodes
{
	/// <summary>
	/// Ссылка
	/// </summary>
	public class ReferenceNode : IXmlConvertable
	{
		public int Id { get; set; }
		private bool skipId;
		public List<PropertyNode> Properties { get; set; }

		public ReferenceNode()
		{
			Properties = new List<PropertyNode>();
		}

		public ReferenceNode(params PropertyNode[] properties)
			: this()
		{
			skipId = true;
			Properties.AddRange(properties);
		}

		public ReferenceNode(int id, params PropertyNode[] properties)
			: this()
		{
			Id = id;
			Properties.AddRange(properties);
		}

		public XElement ToXml()
		{
			var xml = new XElement("Ссылка");
			if(!skipId)
				xml.Add(
					new XAttribute("Нпп", Id)
				);
			Properties.ForEach(prop => xml.Add(prop.ToXml()));
			return xml;
		}
	}
}
