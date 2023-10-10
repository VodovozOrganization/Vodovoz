using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class LateArrivalReasonsJournalRegistrar : ColumnsConfigRegistrarBase<LateArrivalReasonsJournalViewModel, LateArrivalReasonsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<LateArrivalReasonsJournalNode> config) =>
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
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
						c.ForegroundGdk = n.IsArchive ? GdkColors.InsensitiveText : GdkColors.PrimaryText)
				.Finish();
	}
}
