using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Gamma.Utilities;

namespace Vodovoz.Tools.CommerceML.Nodes
{
	public struct PropertyOfGoods
	{
		public Guid Guid;
		public string Name;
		public PropertyTypeValue Type;

		public string TypeTitle => Type.GetEnumTitle();

		public PropertyOfGoods(string guid, string name, PropertyTypeValue typeValue)
		{
			Guid = Guid.Parse(guid);
			Name = name;
			Type = typeValue;
		}

		public XElement ToXml()
		{
			var xml = new XElement("Свойство");
			xml.Add(new XElement("Ид", Guid));
			xml.Add(new XElement("Наименование", Name));
			xml.Add(new XElement("ТипЗначений", TypeTitle));

			return xml;
		}

		public XElement ToValueXml(object value)
		{
			var xml = new XElement("ЗначенияСвойства");
			xml.Add(new XElement("Ид", Guid));
			xml.Add(new XElement("Значение", value));

			return xml;

		}
	}

	public enum PropertyTypeValue
	{
		[Display(Name = "Строка")]
		String,
		[Display(Name = "Число")]
		Number,
		[Display(Name = "ДатаВремя")]
		DateTime
	}
}
