using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints.ComplaintResults;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints.ComplaintResults;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ComplaintResultsOfCounterpartyJournalRegistrar : ColumnsConfigRegistrarBase<ComplaintResultsOfCounterpartyJournalViewModel, ComplaintResultsOfCounterpartyJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ComplaintResultsOfCounterpartyJournalNode> config) =>
			config.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Name)
				.AddColumn("В архиве?")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(node => node.IsArchive)
					.Editing(false)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((cell, node) => cell.ForegroundGdk = node.IsArchive ? GdkColors.DarkGrayColor : GdkColors.BlackColor)
				.Finish();
	}
}
