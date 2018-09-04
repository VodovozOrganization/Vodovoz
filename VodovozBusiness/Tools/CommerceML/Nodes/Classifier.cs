using System;
using System.Xml.Linq;

namespace Vodovoz.Tools.CommerceML.Nodes
{
	public class Classifier: GuidNodeBase, IXmlConvertable 
	{

		public Classifier(Export export)
		{
			myExport = export;
			myExport.ProductGroups = groups = new Groups(export);
		}

		Export myExport;
		Groups groups;

		public override Guid Guid => Guid.Parse("a1a11de6-03b3-4d98-b98d-4d4c0601aa9e");
		public string Name = "Классификатор (Основной каталог товаров)";

		public static PropertyOfGoods PropertyColor = new PropertyOfGoods("fbc4d86b-1202-47d6-bbd6-6820134f3e68", "Цвет оборудования", PropertyTypeValue.String);

		public XElement ToXml()
		{
			var xml = new XElement("Классификатор");
			xml.Add(new XElement("Ид", Guid));
			xml.Add(new XElement("Наименование", Name));
			xml.Add(myExport.DefaultOwner.ToXml());
			xml.Add(groups.ToXml());
			xml.Add(new XElement("Свойства",
			                     PropertyColor.ToXml()
			                    ));

			return xml;
		}
	}
}
