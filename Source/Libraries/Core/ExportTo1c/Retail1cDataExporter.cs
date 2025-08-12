using Gamma.Utilities;
using QS.Dialog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Vodovoz.Core.Domain.Attributes;
using Vodovoz.Domain.Orders;

namespace ExportTo1c.Library
{
	public static class Retail1cDataExporter
	{
		public static XElement CreateRetailXml(
			IList<Order> orders,
			DateTime startOfYesterday,
			DateTime endOfYesterday,
			string organizationInn,
			CancellationToken cancellationToken,
			IProgressBarDisplayable progressBarDisplayable = null)
		{
			return new XElement("ФайлОбмена",
				new XAttribute("НачалоПериодаВыгрузки", startOfYesterday.ToString("yyyy-MM-ddTHH:mm:ss")),
				new XAttribute("ОкончаниеПериодаВыгрузки", endOfYesterday.ToString("yyyy-MM-ddTHH:mm:ss")),
				new XElement("Организация",
					new XAttribute("ИНН", organizationInn),
					new XElement("Продажи",
						CreateExportRetailRows(orders, cancellationToken, progressBarDisplayable)
					)
				)
			);
		}

		private static IList<XElement> CreateExportRetailRows(IList<Order> orders, CancellationToken cancellationToken, IProgressBarDisplayable progressBarDisplayable)
		{
			var ordersCount = orders.Count();

			progressBarDisplayable?.Start(ordersCount, 0, "Выгрузка розницы");

			var xElements = new List<XElement>();

			int i = 0;

			while(i < ordersCount)
			{
				var order = orders[i];

				var items = order.OrderItems;

				foreach(var item in items)
				{

					var rowItem = new XElement("Строка",
						new XAttribute("Код", item.Nomenclature.Code1c),
						new XAttribute("Номенклатура", item.Nomenclature.Name),
						new XAttribute("Количество", item.CurrentCount.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("ЕдиницаИзмерения", item.Nomenclature.Unit.Name),
						new XAttribute("Цена", item.Price.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("Сумма", item.Sum.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("СуммаНДС", item.CurrentNDS.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("СтавкаНДС", item.Nomenclature.VAT.GetAttribute<Value1cComplexAutomation>().Value));

					xElements.Add(rowItem);
		
					cancellationToken.ThrowIfCancellationRequested();					
				}

				i++;

				progressBarDisplayable?.Add(1, $"Заказ {i}/{ordersCount}");
			}

			progressBarDisplayable?.Update("Выгрузка розницы завершена. Сохранение в файл...");

			return xElements;
		}
	}
}
