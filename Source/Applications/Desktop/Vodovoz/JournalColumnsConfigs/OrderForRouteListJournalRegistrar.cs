using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QSProjectsLib;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class OrderForRouteListJournalRegistrar : ColumnsConfigRegistrarBase<OrderForRouteListJournalViewModel, OrderForRouteListJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OrderForRouteListJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Район доставки").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DistrictName)
				.AddColumn("Адрес").AddTextRenderer(node => node.Address)
				.AddColumn("Время").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DeliveryTime)
				.AddColumn("Ожидает до").AddTimeRenderer(node => node.WaitUntilTime)
				.AddColumn("Статус").AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
				.AddColumn("Бутыли").AddTextRenderer(node => $"{node.BottleAmount:N0}")
				.AddColumn("Кол-во с/о").AddTextRenderer(node => $"{node.SanitisationAmount:N0}")
				.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
				.AddColumn("Клиент").AddTextRenderer(node => node.Counterparty)
				.AddColumn("Автор").AddTextRenderer(node => node.Author)
				.RowCells().AddSetter<CellRendererText>((c, n) =>
				{
					var color = GdkColors.PrimaryText;

					if(n.StatusEnum == OrderStatus.Canceled
						|| n.StatusEnum == OrderStatus.DeliveryCanceled)
					{
						color = GdkColors.InsensitiveText;
					}

					if(n.StatusEnum == OrderStatus.Closed)
					{
						color = GdkColors.SuccessText;
					}

					if(n.StatusEnum == OrderStatus.NotDelivered)
					{
						color = GdkColors.InfoText;
					}

					c.ForegroundGdk = color;
				})
				.Finish();
	}
}
