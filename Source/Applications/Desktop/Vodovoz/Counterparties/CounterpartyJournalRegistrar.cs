using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class CounterpartyJournalRegistrar : ColumnsConfigRegistrarBase<CounterpartyJournalViewModel, CounterpartyJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<CounterpartyJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Вн.номер").AddTextRenderer(x => x.InternalId.ToString())
				.AddColumn("Тег").AddTextRenderer(x => x.Tags, useMarkup: true)
				.AddColumn("Контрагент").AddTextRenderer(node => node.Name).WrapWidth(450).WrapMode(WrapMode.WordChar)
				.AddColumn("Телефоны").AddTextRenderer(x => x.Phones)
				.AddColumn("ИНН").AddTextRenderer(x => x.INN)
				.AddColumn("Договора").AddTextRenderer(x => x.Contracts)
				.AddColumn("Точки доставки").AddTextRenderer(x => x.Addresses)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk =
					n.IsArhive || n.IsLiquidating || !n.Sensitive
					? Rc.GetStyle(Startup.MainWin).Foreground(StateType.Insensitive)
					: Rc.GetStyle(Startup.MainWin).Foreground(StateType.Normal)
				)
				.Finish();
	}
}
