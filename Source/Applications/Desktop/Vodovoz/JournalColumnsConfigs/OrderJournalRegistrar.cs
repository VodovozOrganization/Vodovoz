using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QSProjectsLib;
using System;
using System.Globalization;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class OrderJournalRegistrar : ColumnsConfigRegistrarBase<OrderJournalViewModel, OrderJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OrderJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Номер УПД")
					.AddTextRenderer(node => node.UpdDocumentName)
				.AddColumn("Дата").AddTextRenderer(node => node.Date != null ? ((DateTime)node.Date).ToString("d") : string.Empty)
				.AddColumn("Автор").AddTextRenderer(node => node.Author)
				.AddColumn("Время").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DeliveryTime)
				.AddColumn("Ожидает до").AddTimeRenderer(node => node.WaitUntilTime)
				.AddColumn("Статус").AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
				.AddColumn("Тип").AddTextRenderer(node => node.ViewType)
					.WrapMode(WrapMode.WordChar)
					.WrapWidth(100)
				.AddColumn("Бутыли").AddTextRenderer(node => $"{node.BottleAmount:N0}")
				.AddColumn("Кол-во с/о").AddTextRenderer(node => $"{node.SanitisationAmount:N0}")
				.AddColumn("Клиент").AddTextRenderer(node => node.Counterparty)
				.AddColumn("ИНН").AddTextRenderer(node => node.Inn)
				.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
				.AddColumn("Статус оплаты").AddTextRenderer(x =>
					(x.OrderPaymentStatus != OrderPaymentStatus.None) ? x.OrderPaymentStatus.GetEnumTitle() : "")
				.AddColumn("Статус документооборота").AddTextRenderer(node => node.EdoDocFlowStatusString)
				.AddColumn("Район доставки").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DistrictName)
				.AddColumn("Адрес").AddTextRenderer(node => node.Address)
				.AddColumn("Изменил").AddTextRenderer(node => node.LastEditor)
				.AddColumn("Послед. изменения").AddTextRenderer(node =>
					node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString(CultureInfo.CurrentCulture) : string.Empty)
				.AddColumn("Номер звонка").AddTextRenderer(node => node.DriverCallId.ToString())
				.AddColumn("OnLine заказ №").AddTextRenderer(node => node.OnLineNumber)
				.AddColumn("Номер заказа интернет-магазина").AddTextRenderer(node => node.EShopNumber)
				.RowCells().AddSetter<CellRendererText>((c, n) =>
				{
					var color = GdkColors.PrimaryText;

					if(n.StatusEnum == OrderStatus.Canceled
						|| n.StatusEnum == OrderStatus.DeliveryCanceled
						|| !n.Sensitive)
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
