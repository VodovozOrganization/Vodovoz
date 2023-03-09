﻿using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QSProjectsLib;
using Vodovoz.JournalNodes;
using Vodovoz.Representations;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class SelfDeliveriesJournalRegistrar : ColumnsConfigRegistrarBase<SelfDeliveriesJournalViewModel, SelfDeliveryJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<SelfDeliveryJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
				.AddColumn("Автор").AddTextRenderer(node => node.Author)
				.AddColumn("Статус").AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
				.AddColumn("Тип оплаты").AddTextRenderer(node => node.PaymentTypeEnum.GetEnumTitle())
				.AddColumn("Бутыли").AddTextRenderer(node => $"{node.BottleAmount:N0}")
				.AddColumn("Клиент").AddTextRenderer(node => node.Counterparty)
				.AddColumn("Вариант оплаты").AddTextRenderer(node => node.PayOption)
				.AddColumn("Сумма безнал").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.OrderCashlessSumTotal))
				.AddColumn("Сумма нал").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.OrderCashSumTotal))
				.AddColumn("Из них возврат").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.OrderReturnSum))
				.AddColumn("Касса приход").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.CashPaid))
				.AddColumn("Касса возврат").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.CashReturn))
				.AddColumn("Касса итог").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.CashTotal))
				.AddColumn("Расхождение по нал.").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.TotalCashDiff))
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
				.Finish();
	}
}
