using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class BulkEmailEventReasonJournalRegistrar : ColumnsConfigRegistrarBase<BulkEmailEventReasonJournalViewModel, BulkEmailEventReasonJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<BulkEmailEventReasonJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Архивный").AddToggleRenderer(node => node.IsArchive).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
