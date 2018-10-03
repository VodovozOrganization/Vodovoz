using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Vodovoz.Repository;
using Vodovoz.Repository.Store;

namespace Vodovoz.Tools.CommerceML.Nodes
{
	public class Offers : IXmlConvertable 
	{
		Dictionary<int, decimal> amounts;

		public Offers(Export export)
		{
			myExport = export;
			myExport.OnProgressPlusOneTask("Выгружаем наличие на складе");

			var warehouses = WarehouseRepository.WarehousesForPublishOnlineStore(myExport.UOW);
			var nomenclatureIds = myExport.Catalog.Goods.NomenclatureIds;
			var warehousesIds = warehouses.Select(x => x.Id).ToArray();

			amounts = StockRepository.NomenclatureInStock(myExport.UOW, warehousesIds, nomenclatureIds);
		}

		Export myExport;

		public XElement ToXml()
		{
			var xml = new XElement("Предложения");
			foreach(var good in myExport.Catalog.Goods.Nomenclatures)
			{
				if(!amounts.ContainsKey(good.Id))
					continue;

				var goodxml = new XElement("Предложение");
				goodxml.Add(new XElement("Ид", good.OnlineStoreGuid));
				goodxml.Add(new XElement("Штрихкод"));
				goodxml.Add(new XElement("Наименование", good.Name));
				goodxml.Add(new XElement("Цены", 
				                         new XElement("Цена", 
				                                      new XElement("Представление", String.Format("{0:N} руб. за {1}", good.GetPrice(1), good.Unit?.Name)),
				                                      new XElement("ИдТипаЦены", myExport.DefaultPriceGuid),
				                                      new XElement("ЦенаЗаЕдиницу", good.GetPrice(1)),
				                                      new XElement("Валюта", "руб"),
				                                      new XElement("Единица", good.Unit?.Name),
				                                      new XElement("Коэффициент", 1)
				                                     )));
				goodxml.Add(new XElement("Количество", XmlConvert.ToString(amounts[good.Id])));
				xml.Add(goodxml);
			}
			return xml;
		}
	}
}
