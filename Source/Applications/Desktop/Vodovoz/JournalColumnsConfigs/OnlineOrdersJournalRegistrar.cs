using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Utilities;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalColumnsConfigs
{
	public class OnlineOrdersJournalRegistrar : ColumnsConfigRegistrarBase<OnlineOrdersJournalViewModel, OnlineOrdersJournalNode>
	{
		private static readonly Pixbuf _emptyImg = null;
		private static readonly Pixbuf _greenCircle = new Pixbuf(typeof(Startup).Assembly, "Vodovoz.icons.common.green_circle16.png");

		public override IColumnsConfig Configure(FluentColumnsConfig<OnlineOrdersJournalNode> config) =>
			config.AddColumn("Номер")
					.AddTextRenderer(node => node.Id.ToString())
					.AddPixbufRenderer(node =>
						node.OnlineOrderStatus == OnlineOrderStatus.New
						&& string.IsNullOrWhiteSpace(node.ManagerWorkWith)
							? _greenCircle
							: _emptyImg)
				.AddColumn("Дата доставки").AddTextRenderer(node => node.DeliveryDate.ToShortDateString())
				.AddColumn("Время доставки").AddTextRenderer(
					node => node.IsSelfDelivery ? "-" : node.DeliveryTime)
				.AddColumn("Оформленный заказ").AddTextRenderer(node => node.OrderId.ToString())
				.AddColumn("Статус").AddTextRenderer(node => node.OnlineOrderStatus.GetEnumDisplayName(false))
				.AddColumn("Клиент").AddTextRenderer(node => node.CounterpartyName)
				.AddColumn("Адрес").AddTextRenderer(node => node.CompiledAddress)
				.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.OnlineOrderSum))
				.AddColumn("Источник").AddTextRenderer(node => node.Source.GetEnumDisplayName(false))
				.AddColumn("Статус оплаты").AddTextRenderer(node => node.OnlineOrderPaymentStatus.GetEnumDisplayName(false))
				.AddColumn("Номер оплаты").AddTextRenderer(node => node.OnlinePayment.ToString())
				.AddColumn("В работе").AddTextRenderer(node => node.ManagerWorkWith)
				.RowCells().AddSetter<CellRendererText>((cell, node) =>
				{
					var color = GdkColors.PrimaryText;
					
					if(node.OnlineOrderStatus == OnlineOrderStatus.New && node.IsFastDelivery)
					{
						color = GdkColors.DangerText;
					}
					else if(node.OnlineOrderStatus == OnlineOrderStatus.New && node.IsNeedConfirmationByCall)
					{
						color = GdkColors.Orange;
					}

					if(node.OnlineOrderStatus == OnlineOrderStatus.Canceled)
					{
						color = GdkColors.InsensitiveText;
					}

					cell.ForegroundGdk = color;
				})
				.Finish();
	}
}
