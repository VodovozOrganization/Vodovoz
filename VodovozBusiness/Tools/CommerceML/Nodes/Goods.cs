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
		}

		Export myExport;

		public XElement ToXml()
		{
			var xml = new XElement("Товары");
			foreach(var good in Nomenclatures)
			{
				var goodxml = new XElement("Товар");
				goodxml.Add(new XElement("Ид", good.GetOrCreateGuid(myExport.UOW)));
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
				//Пока пропущено картинки и свойства.
				goodxml.Add(new XElement("СтавкиНалогов", 
				                         new XElement("СтавкаНалога", 
				                                      new XElement("Наименование", "НДС"),
				                                      new XElement("Ставка", good.VAT.GetEnumTitle()) )));
				xml.Add(goodxml);
			}
			return xml;
		}

		public int[] NomenclatureIds{
			get{
				return Nomenclatures.Select(x => x.Id).ToArray();
			}
		}
	}
}
