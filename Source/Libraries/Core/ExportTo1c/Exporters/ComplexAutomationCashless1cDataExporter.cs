using Gamma.Utilities;
using QS.Dialog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Vodovoz.Core.Domain.Attributes;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace ExportTo1c.Library.Exporters
{
	/// <summary>
	/// Экспорт данных для 1С: Комплексная автоматизация - Безнал
	/// </summary>
	public class ComplexAutomationCashless1cDataExporter : IDataExporterFor1c
	{
		public XElement CreateXml(
			IList<Order> orders,
			DateTime startDate,
			DateTime endDate,
			Organization organization,
			CancellationToken cancellationToken,
			IProgressBarDisplayable progressBarDisplayable = null)
		{
			return new XElement("ФайлОбмена",
				new XAttribute("НачалоПериодаВыгрузки", startDate.ToString("yyyy-MM-ddTHH:mm:ss")),
				new XAttribute("ОкончаниеПериодаВыгрузки", endDate.ToString("yyyy-MM-ddTHH:mm:ss")),
				new XElement("Организация", new XAttribute("ИНН", organization.INN)),
				CreateCashlessExportRows(orders, organization, startDate, endDate, progressBarDisplayable, cancellationToken)
				);
		}

		private static IList<XElement> CreateCashlessExportRows(
			IList<Order> orders,
			Organization organization,
			DateTime startDate,
			DateTime endDate,
			IProgressBarDisplayable progressBarDisplayable,
			CancellationToken cancellationToken)
		{
			var ordersCount = orders.Count();

			progressBarDisplayable?.Start(ordersCount, 0, $"Выгрузка безнала");

			var ordersElements = new List<XElement>();

			var i = 0;

			while(i < ordersCount)
			{
				var order = orders[i];

				var orderElement = new XElement
				(
					"Заказ",
					new XAttribute("Дата", order.DeliveryDate?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""),
					new XAttribute("Номер", order.Id),
					new XAttribute("КонтрагентИНН", order.Client.INN),
					new XAttribute("Договор", $"{order.Contract.Number} от {order.Contract.IssueDate:d}")
				);

				var salesElement = new XElement("Продажи");

				var items = order.OrderItems;

				foreach(var item in items)
				{
					var rowElement = new XElement
					(
						"Строка",
						new XAttribute("Код", item.Nomenclature.Code1c),
						new XAttribute("Номенклатура", item.Nomenclature.Name),
						new XAttribute("НоменклатураОфициальноеНазвание", item.Nomenclature.OfficialName),
						new XAttribute("Количество", item.CurrentCount.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("ЕдиницаИзмерения", item.Nomenclature.Unit.Name),
						new XAttribute("Цена", item.Price.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("Сумма", item.Sum.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("СуммаНДС", item.CurrentNDS.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("СтавкаНДС", item.Nomenclature.VAT.GetAttribute<Value1cComplexAutomation>().Value),
						new XAttribute("Безнал", item.Order.PaymentType != PaymentType.Cash),
						new XAttribute("КатегорияНоменклатуры", item.Nomenclature.Category.GetEnumTitle()),
						new XAttribute("ОдноразоваяТара", item.Nomenclature.IsDisposableTare)
					);

					salesElement.Add(rowElement);

					cancellationToken.ThrowIfCancellationRequested();
				}

				orderElement.Add(salesElement);

				ordersElements.Add(orderElement);

				i++;

				progressBarDisplayable?.Add(1, $"Выгрузка безнала для {organization.Name} c {startDate:d} по {endDate:d}. Заказ {i}/{ordersCount}");
			}

			progressBarDisplayable?.Update("Выгрузка безнала завершена.");

			return ordersElements;
		}
	}
}
