using Gamma.Utilities;
using QS.Dialog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace ExportTo1c.Library.Exporters
{
	/// <summary>
	/// Экспорт данных для 1С: Комплексная автоматизация - Безнал
	/// </summary>
	public class ComplexAutomationCashless1cDataExporter : IDataExporterFor1c<Order>
	{
		private readonly Organization _organization;
		private readonly DateTime _startDate;
		private readonly DateTime _endDate;
		private readonly IProgressBarDisplayable _progressBarDisplayable;

		public ComplexAutomationCashless1cDataExporter(Organization organization, DateTime startDate, DateTime endDate,	IProgressBarDisplayable progressBarDisplayable = null)
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
			return new XElement("ФайлОбмена",
				new XAttribute("НачалоПериодаВыгрузки", _startDate.ToString("yyyy-MM-ddTHH:mm:ss")),
				new XAttribute("ОкончаниеПериодаВыгрузки", _endDate.ToString("yyyy-MM-ddTHH:mm:ss")),
				new XElement("Организация", new XAttribute("ИНН", _organization.INN)),
				CreateCashlessExportRows(sourceList, _organization, _startDate, _endDate, _progressBarDisplayable, cancellationToken)
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

			var counterDocumentsTypes = new[] { OrderDocumentType.UPD, OrderDocumentType.SpecialUPD };

			var ordersElements = new List<XElement>();

			var i = 0;

			while(i < ordersCount)
			{
				var order = orders[i];

				var updNum = order.OrderDocuments
					.FirstOrDefault(od => counterDocumentsTypes.Contains(od.Type) && od.DocumentOrganizationCounter != null)
					?.DocumentOrganizationCounter
					?.DocumentNumber
					?? order.Id.ToString();

				var orderElement = new XElement
				(
					"Заказ",
					new XAttribute("Дата", order.DeliveryDate?.ToString("yyyy-MM-ddTHH:mm:ss") ?? ""),
					new XAttribute("Номер", order.Id),
					new XAttribute("НомерУПД", updNum),
					new XAttribute("КонтрагентИНН", order.Client.INN),
					new XAttribute("Договор", $"{order.Contract.Number} от {order.Contract.IssueDate:d}"),
					new XAttribute("Статус", order.OrderStatus.GetEnumTitle())
				);

				var salesElement = new XElement("Продажи");

				var items = order.OrderItems;
								
				foreach(var item in items)
				{
					var vatRateVersion = item.Nomenclature.GetActualVatRateVersion(order.BillDate);

					if(vatRateVersion == null)
					{
						throw new InvalidOperationException($"У номенклатуры #{item.Id} отсутствует версия НДС на дату счета {order.BillDate}");
					}
					
					var rowElement = new XElement
					(
						"Строка",
						new XAttribute("Код", item.Nomenclature.Code1c),
						new XAttribute("Номенклатура", item.Nomenclature.Name),
						new XAttribute("НоменклатураОфициальноеНазвание", item.Nomenclature.OfficialName),
						new XAttribute("Количество", item.CurrentCount.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("ЕдиницаИзмерения", item.Nomenclature.Unit.Name),
						new XAttribute("Цена", item.Price.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("Сумма", item.ActualSum.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("СуммаНДС", item.CurrentNDS.ToString("F2", CultureInfo.InvariantCulture)),
						new XAttribute("СтавкаНДС", vatRateVersion.VatRate.GetValue1cComplexAutomation()),
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
