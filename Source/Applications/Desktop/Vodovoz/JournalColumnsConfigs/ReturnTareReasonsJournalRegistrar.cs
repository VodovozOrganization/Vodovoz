using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ReturnTareReasonsJournalRegistrar : ColumnsConfigRegistrarBase<ReturnTareReasonsJournalViewModel, ReturnTareReasonsJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorDarkGray = new Color(0x80, 0x80, 0x80);

		public override IColumnsConfig Configure(FluentColumnsConfig<ReturnTareReasonsJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Причина")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
					.XAlign(0.5f)
				.AddColumn("В архиве?")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(n => n.IsArchive)
					.Editing()
					.XAlign(0.5f)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGray : _colorBlack)
				.Finish();
	}
}
