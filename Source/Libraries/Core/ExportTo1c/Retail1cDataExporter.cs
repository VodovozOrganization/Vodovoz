using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Gamma.Utilities;
using Vodovoz.Core.Domain.Attributes;
using Vodovoz.Domain.Orders;

namespace ExportTo1c.Library
{
	public static class Retail1cDataExporter
	{
		public static XElement CreateRetailXml(IEnumerable<OrderItem> orderItems, DateTime startOfYesterday, DateTime endOfYesterday)
		{
			return new XElement("ФайлОбмена",
				new XAttribute("НачалоПериодаВыгрузки", startOfYesterday.ToString("yyyy-MM-ddTHH:mm:ss")),
				new XAttribute("ОкончаниеПериодаВыгрузки", endOfYesterday.ToString("yyyy-MM-ddTHH:mm:ss")),
				new XElement("Организация",
					new XAttribute("ИНН", ""),
					new XElement("Продажи",
						CreateExportRetailRows(orderItems)
					)
				)
			);
		}
		
		private static IEnumerable<XElement> CreateExportRetailRows(IEnumerable<OrderItem> orderItems)
		{
			return orderItems.Select(item => new XElement("Строка",
				new XAttribute("Код", item.Nomenclature.Code1c),
				new XAttribute("Номенклатура", item.Nomenclature.Name),
				new XAttribute("Количество", item.CurrentCount.ToString("F2", CultureInfo.InvariantCulture)),
				new XAttribute("ЕдиницаИзмерения", item.Nomenclature.Unit.Name),
				new XAttribute("Цена", item.Price.ToString("F2", CultureInfo.InvariantCulture)),
				new XAttribute("Сумма", item.Sum.ToString("F2", CultureInfo.InvariantCulture)),
				new XAttribute("СуммаНДС", item.CurrentNDS.ToString("F2", CultureInfo.InvariantCulture)),
				new XAttribute("СтавкаНДС", item.Nomenclature.VAT.GetAttribute<Value1cComplexAutomation>().Value)
			));
		}
	}
}
