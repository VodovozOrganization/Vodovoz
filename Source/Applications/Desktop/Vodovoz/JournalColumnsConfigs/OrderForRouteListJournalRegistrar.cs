﻿using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QSProjectsLib;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class OrderForRouteListJournalRegistrar : ColumnsConfigRegistrarBase<OrderForRouteListJournalViewModel, OrderForRouteListJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OrderForRouteListJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Район доставки").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DistrictName)
				.AddColumn("Адрес").AddTextRenderer(node => node.Address)
				.AddColumn("Время").AddTextRenderer(node => node.IsSelfDelivery ? "-" : node.DeliveryTime)
				.AddColumn("Статус").AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
				.AddColumn("Бутыли").AddTextRenderer(node => $"{node.BottleAmount:N0}")
				.AddColumn("Кол-во с/о").AddTextRenderer(node => $"{node.SanitisationAmount:N0}")
				.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
				.AddColumn("Клиент").AddTextRenderer(node => node.Counterparty)
				.AddColumn("Автор").AddTextRenderer(node => node.Author)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
				.Finish();
	}
}
