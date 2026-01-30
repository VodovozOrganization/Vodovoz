using Gamma.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;

namespace ExportTo1c.Library.Exporters
{
	/// <summary>
	/// Экспорт изменённых заказов для 1С Апи
	/// </summary>
	public class Api1cChangesExporter : IDataExporterFor1c<OrderTo1cExport>
	{
		private readonly DateTime _exportDate;

		public Api1cChangesExporter(DateTime exportDate)
		{
			_exportDate = exportDate;
		}

		public XElement CreateXml(
			IList<OrderTo1cExport> sourceList,
			CancellationToken cancellationToken)
		{
			return new XElement("ФайлОбмена",
				new XAttribute("НачалоПериодаВыгрузки", _exportDate.ToString("yyyy-MM-ddTHH:mm:ss")),
				CreateCashlessExportRows(sourceList, cancellationToken)
				);
		}

		private static IList<XElement> CreateCashlessExportRows(
			IList<OrderTo1cExport> cnahgedOrders,
			CancellationToken cancellationToken)
		{
			var counterDocumentsTypes = new[] { OrderDocumentType.UPD, OrderDocumentType.SpecialUPD };

			var ordersElements = new List<XElement>();

			foreach(var cnahgedOrder in cnahgedOrders)
			{
				var order = cnahgedOrder.Order;

				if(order == null)
				{
					var deletedOrderElement = new XElement
					(
						"Заказ",
						new XAttribute("Номер", cnahgedOrder.OrderId),
						new XAttribute("Удалён", true)
					);

					ordersElements.Add(deletedOrderElement);

					continue;
				}

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
					new XAttribute("Договор", $"{order.Contract.Number} от {order.Contract.IssueDate:d}")
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
						new XAttribute("Сумма", item.Sum.ToString("F2", CultureInfo.InvariantCulture)),
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
			}

			return ordersElements;
		}
	}
}
