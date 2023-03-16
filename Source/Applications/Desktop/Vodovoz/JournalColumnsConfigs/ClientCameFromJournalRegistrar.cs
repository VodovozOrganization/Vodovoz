using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal class ClientCameFromJournalRegistrar : ColumnsConfigRegistrarBase<ClientCameFromJournalViewModel, ClientCameFromJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ClientCameFromJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название").AddTextRenderer(n => n.Name)
				.AddColumn("В архиве").AddTextRenderer(n => n.IsArchive ? "Да" : "Нет")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
				.Finish();
	}
}
