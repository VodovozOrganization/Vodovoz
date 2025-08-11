using System.Xml.Linq;

namespace ExportTo1c.Library.ExportNodes
{
	/// <summary>
	/// Значение параметра
	/// </summary>
	public class PropertyValueParameterNode : PropertyNode
	{
		public PropertyValueParameterNode(string name, string type, string value)
			: base(name, type, value)
		{
		}

		public override XElement ToXml()
		{
			var xml = new XElement("ЗначениеПараметра",
				new XAttribute("Имя", Name),
				new XAttribute("Тип", Type),
				ValueOrReference.ToXml()
			);
			foreach(var xattr in AdditionalAttributes)
			{
				xml.Add(xattr);
			}

			return xml;
		}
	}
}
