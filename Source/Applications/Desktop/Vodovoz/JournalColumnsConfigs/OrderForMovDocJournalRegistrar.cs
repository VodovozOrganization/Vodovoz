using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QSProjectsLib;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.JournalViewModels;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class OrderForMovDocJournalRegistrar : ColumnsConfigRegistrarBase<OrderForMovDocJournalViewModel, OrderForMovDocJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OrderForMovDocJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
				.AddColumn("Статус").AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
				.AddColumn("Бутыли").AddTextRenderer(node => $"{node.BottleAmount:N0}")
				.AddColumn("Клиент")
					.AddTextRenderer(node => node.Counterparty)
					.WrapMode(WrapMode.WordChar)
					.WrapWidth(400)
				.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
				.AddColumn("OnLine заказ №").AddTextRenderer(node => node.OnLineNumber)
				.AddColumn("Номер заказа ИМ").AddTextRenderer(node => node.EShopNumber)
				.AddColumn("Адрес").AddTextRenderer(node => node.Address)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
				.Finish();
	}
}
