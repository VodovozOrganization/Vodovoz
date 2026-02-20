using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Utilities;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using WrapMode = Pango.WrapMode;

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
				.AddColumn("Тип").AddTextRenderer(node => node.EntityTypeString)
				.AddColumn("Дата создания").AddTextRenderer(node => node.CreationDate.ToString("G"))
				.AddColumn("Дата доставки").AddTextRenderer(node =>
						node.DeliveryDate.HasValue ? node.DeliveryDate.Value.ToShortDateString() : string.Empty)
				.AddColumn("Время доставки").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DeliveryTime)
				.AddColumn("Оформленный\nзаказ(ы)").AddTextRenderer(node => node.OrdersIds)
				.AddColumn("Статус").AddTextRenderer(node => node.Status)
				.AddColumn("Клиент")
					.AddTextRenderer(node => node.CounterpartyName)
					.WrapMode(WrapMode.Word)
					.WrapWidth(350)
				.AddColumn("Адрес")
					.AddTextRenderer(node => node.CompiledAddress)
					.WrapMode(WrapMode.Word)
					.WrapWidth(500)
				.AddColumn("Сумма").AddTextRenderer(node =>
						node.OnlineOrderSum.HasValue
							? CurrencyWorks.GetShortCurrencyString(node.OnlineOrderSum.Value)
							: string.Empty)
				.AddColumn("Источник").AddTextRenderer(node => node.Source.GetEnumDisplayName(false))
				.AddColumn("Статус оплаты").AddTextRenderer(node =>
						node.OnlineOrderPaymentStatus.HasValue
							? node.OnlineOrderPaymentStatus.Value.GetEnumDisplayName(false)
							: string.Empty)
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
