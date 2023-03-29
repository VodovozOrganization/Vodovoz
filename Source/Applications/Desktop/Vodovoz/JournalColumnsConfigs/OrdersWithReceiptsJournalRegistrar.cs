using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using Vodovoz.ViewModels.Journals.JournalViewModels.Roboats;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class OrdersWithReceiptsJournalRegistrar : ColumnsConfigRegistrarBase<OrdersWithReceiptsJournalViewModel, CashReceiptNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<CashReceiptNode> config) =>
			config
				.AddColumn("Код заказа").AddNumericRenderer(node => node.OrderId)
				.AddColumn("Дата доставки").AddTextRenderer(node => node.DeliveryDate.HasValue ? node.DeliveryDate.Value.ToString("dd.MM.yyyy") : "")
				.AddColumn("Сумма").AddNumericRenderer(node => node.OrderSum)
				.AddColumn("Тип оплаты").AddTextRenderer(node => node.PaymentType)
				.AddColumn("Самовывоз").AddToggleRenderer(node => node.IsSelfdelivery).Editing(false)
				.AddColumn("Код чека").AddNumericRenderer(node => node.ReceiptId)
				.AddColumn("Время чека").AddTextRenderer(node => node.ReceiptTime.HasValue ? node.ReceiptTime.Value.ToString("dd.MM.yyyy HH:mm:ss") : "")
				.AddColumn("Статус").AddTextRenderer(node => node.Status)
				.AddColumn("МЛ").AddNumericRenderer(node => node.RouteListId).Digits(0)
				.AddColumn("Водитель").AddTextRenderer(node => node.DriverFIO)
				.AddColumn("Причина не отскани-\nрованных бутылей").AddTextRenderer(node => node.UnscannedReason).WrapMode(Pango.WrapMode.Word).WrapWidth(400)
				.AddColumn("Описание ошибки").AddTextRenderer(node => node.ErrorDescription).WrapMode(Pango.WrapMode.Word).WrapWidth(400)
				.AddColumn("")
				.RowCells()
				.Finish();
	}
}
