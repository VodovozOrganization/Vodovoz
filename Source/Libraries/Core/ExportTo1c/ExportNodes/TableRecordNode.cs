using System.Collections.Generic;
using System.Xml.Linq;
using Vodovoz.Tools;

namespace ExportTo1c.Library.ExportNodes
{
	/// <summary>
	/// Строка таблицы
	/// </summary>
	public class TableRecordNode : IXmlConvertable
	{
		public List<PropertyNode> Properties { get; set; }

		public TableRecordNode()
		{
			Properties = new List<PropertyNode>();
		}

		public XElement ToXml()
		{
			var xml = new XElement("Запись");
			Properties.ForEach(prop => xml.Add(prop.ToXml()));
			return xml;
		}
	}
}
