using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Gamma.Utilities;
using Vodovoz.Domain.Goods;
using Vodovoz.Repository;

namespace Vodovoz.Tools.CommerceML.Nodes
{
	public class Goods : IXmlConvertable 
	{
		public IList<Nomenclature> Nomenclatures { get; private set; }

		public Goods(Export export)
		{
			myExport = export;
			myExport.OnProgressPlusOneTask("Выгружаем товары");

			var groupsIds = myExport.ProductGroups.ToExportIds();
			Nomenclatures = NomenclatureRepository.NomenclatureInGroupsQuery(groupsIds).GetExecutableQueryOver(myExport.UOW.Session).List();
            Nomenclatures.ToList().ForEach(n => n.CreateGuidIfNotExist(export.UOW));
		}

		Export myExport;

		public XElement ToXml()
		{
			var xml = new XElement("Товары");
			foreach(var good in Nomenclatures)
			{
				var goodxml = new XElement("Товар");
				goodxml.Add(new XElement("Ид", good.OnlineStoreGuid));
				goodxml.Add(new XElement("Штрихкод"));
				goodxml.Add(new XElement("Артикул", good.Id));
				goodxml.Add(new XElement("Наименование", good.Name));
				if(good.Unit != null)
				{   //Пропущено так как у нас нет: НаименованиеПолное="Штука" МеждународноеСокращение="PCE"
					var unitxml = new XElement("БазоваяЕдиница", good.Unit.Name);
					if(good.Unit.OKEI != null)
						unitxml.Add(new XAttribute("Код", good.Unit.OKEI));
					goodxml.Add(unitxml); 
				}
				goodxml.Add(new XElement("ПолноеНаименование", good.OfficialName));
				goodxml.Add(new XElement("Группы", new XElement("Ид", good.ProductGroup.OnlineStoreGuid)));

				goodxml.Add(new XElement("Описание", good.Description));

				foreach(var img in good.Images)
				{
					goodxml.Add(new XElement("Картинка", $"import_files/img_{img.Id:0000000}.jpg"));
				}

				var propertiesXml = new XElement("ЗначенияСвойств");
				if(good.EquipmentColor != null)
					propertiesXml.Add(Classifier.PropertyColor.ToValueXml(good.EquipmentColor.Name));

				goodxml.Add(propertiesXml);
				foreach(var characteristic in good.ProductGroup.Characteristics) {
					var value = good.GetPropertyValue(characteristic.ToString());
					propertiesXml.Add(myExport.Classifier.Characteristics[characteristic].ToValueXml(value));
				}

				goodxml.Add(new XElement("СтавкиНалогов", 
				                         new XElement("СтавкаНалога", 
				                                      new XElement("Наименование", "НДС"),
				                                      new XElement("Ставка", good.VAT.GetEnumTitle()) )));

				bool isGoods = Nomenclature.GetCategoriesForGoods().Contains(good.Category);
				goodxml.Add(new XElement("ЗначенияРеквизитов",
				                         makeProps("ВидНоменклатуры", good.CategoryString),
				                         makeProps("ТипНоменклатуры", isGoods ? "Товар" : "Услуга"),
				                         makeProps("Полное наименование", good.OfficialName),
				                         makeProps("Вес", good.Weight)
				                        ));
				xml.Add(goodxml);
			}
			return xml;
		}

		private XElement makeProps(string name, object value)
		{
			return new XElement("ЗначениеРеквизита",
			                    new XElement("Наименование", name),
			                    new XElement("Значение", value)
			                   );
		}

		public int[] NomenclatureIds{
			get{
				return Nomenclatures.Select(x => x.Id).ToArray();
			}
		}
	}
}
