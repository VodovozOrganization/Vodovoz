using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Gamma.Utilities;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Tools.CommerceML.Nodes
{
	public class Classifier: GuidNodeBase, IXmlConvertable 
	{

		public Classifier(Export export)
		{
			myExport = export;
			myExport.ProductGroups = groups = new Groups(export);
			foreach(NomenclatureProperties item in Enum.GetValues(typeof(NomenclatureProperties))) {
				var title = item.GetEnumTitle();
				var guid = item.GetAttribute<OnlineStoreGuidAttribute>().Guid;
				Characteristics.Add(item, new PropertyOfGoods(guid, title, PropertyTypeValue.String));
			}
		}

		Export myExport;
		Groups groups;

		public override Guid Guid => Guid.Parse("a1a11de6-03b3-4d98-b98d-4d4c0601aa9e");
		public string Name = "Классификатор (Основной каталог товаров)";

		public static PropertyOfGoods PropertyColor = new PropertyOfGoods("fbc4d86b-1202-47d6-bbd6-6820134f3e68", "Цвет оборудования", PropertyTypeValue.String);

		public Dictionary<NomenclatureProperties, PropertyOfGoods> Characteristics = new Dictionary<NomenclatureProperties, PropertyOfGoods>();

		public XElement ToXml()
		{
			var xml = new XElement("Классификатор");
			xml.Add(new XElement("Ид", Guid));
			xml.Add(new XElement("Наименование", Name));
			xml.Add(myExport.DefaultOwner.ToXml());
			xml.Add(groups.ToXml());
			xml.Add(new XElement("Свойства",
								Characteristics.Values.Select(x => x.ToXml())
								.Concat( new[] { PropertyColor.ToXml() })
			                    ));
			return xml;
		}
	}
}
