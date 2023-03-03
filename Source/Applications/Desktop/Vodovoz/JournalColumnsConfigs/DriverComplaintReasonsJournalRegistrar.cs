using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class DriverComplaintReasonsJournalRegistrar : ColumnsConfigRegistrarBase<DriverComplaintReasonsJournalViewModel, DriverComplaintReasonJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DriverComplaintReasonJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("Популярная").AddToggleRenderer(x => x.IsPopular).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
