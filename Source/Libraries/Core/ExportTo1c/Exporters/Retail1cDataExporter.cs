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
	/// Экспорт данных розничных продаж для 1С
	/// </summary>
	public class Retail1cDataExporter : IDataExporterFor1c<Order>
	{
		private readonly Organization _organization;
		private readonly DateTime _startDate;
		private readonly DateTime _endDate;
		private readonly IProgressBarDisplayable _progressBarDisplayable;

		public Retail1cDataExporter(
		Organization organization,
		DateTime startDate,
		DateTime endDate,
		IProgressBarDisplayable progressBarDisplayable = null)
		{
			_organization = organization ?? throw new ArgumentNullException(nameof(organization));
			_startDate = startDate;
			_endDate = endDate;
			_progressBarDisplayable = progressBarDisplayable;
		}
		public XElement CreateXml(
			IList<Order> sourceList,
			CancellationToken cancellationToken)
		{
			var datesInRange = GetDatesInRange(_startDate, _endDate);

			var ordersByDate = sourceList
				.Where(o => o.DeliveryDate >= _startDate && o.DeliveryDate <= _endDate)
				.OrderBy(o => o.DeliveryDate)
				.GroupBy(o => o.DeliveryDate)
				.ToDictionary(g => g.Key, g => g.ToList());

			return new XElement("ФайлОбмена",
				new XAttribute("НачалоПериодаВыгрузки", _startDate.ToString("yyyy-MM-ddTHH:mm:ss")),
				new XAttribute("ОкончаниеПериодаВыгрузки", _endDate.ToString("yyyy-MM-ddTHH:mm:ss")),
				new XElement("Организация", new XAttribute("ИНН", _organization.INN)),
				datesInRange.Select(date => new XElement("Дата",
					new XAttribute("Значение", date.ToString("yyyy-MM-ddTHH:mm:ss")),
					new XElement("Продажи",
						ordersByDate.TryGetValue(date, out var dateOrders)
							? CreateExportRetailRows(dateOrders, _organization, _progressBarDisplayable, date, cancellationToken)
							: Enumerable.Empty<XElement>())
				)));
		}

		private static IEnumerable<DateTime> GetDatesInRange(DateTime startDate, DateTime endDate)
		{
			for(var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
			{
				yield return date;
			}
		}

		private static IList<XElement> CreateExportRetailRows(
			IList<Order> orders,
			Organization organization,
			IProgressBarDisplayable progressBarDisplayable,
			DateTime date,
			CancellationToken cancellationToken)
		{
			var ordersCount = orders.Count();

			progressBarDisplayable?.Start(ordersCount, 0, $"Выгрузка розницы");

			var xElements = new List<XElement>();

			int i = 0;

			while(i < ordersCount)
			{
				var order = orders[i];

				var items = order.OrderItems
					.Where(x => x.Price != 0m)
					.Where(x => x.Count > 0m);

				foreach(var item in items)
				{
					var isService = item.Nomenclature.Category == NomenclatureCategory.master
					                || item.Nomenclature.Category == NomenclatureCategory.service;

					var rowItem = new XElement("Строка",
						new XAttribute("Заказ", item.Order.Id),
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
						new XAttribute("КатегорияНоменклатуры", isService ? "Услуга" : "Товар"),
						new XAttribute("ОдноразоваяТара", item.Nomenclature.IsDisposableTare),
						new XAttribute("ТипОплаты",
							order.PaymentByTerminalSource?.GetEnumTitle() ?? order.PaymentByCardFrom?.Name ?? order.PaymentType.GetEnumTitle())
					);

					xElements.Add(rowItem);

					cancellationToken.ThrowIfCancellationRequested();
				}

				i++;

				progressBarDisplayable?.Add(1, $"Выгрузка розницы для {organization.Name} за {date:d}. Заказ {i}/{ordersCount}");
			}

			progressBarDisplayable?.Update("Выгрузка розницы завершена.");

			return xElements;
		}
	}
}
