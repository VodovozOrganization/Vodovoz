using Gamma.ColumnConfig;
using Vodovoz.Presentation.ViewModels.Organisations.Journals;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class BusinessActivitiesJournalRegistrar : ColumnsConfigRegistrarBase<BusinessActivitiesJournalViewModel, BusinessActivityJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<BusinessActivityJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("Архив").AddToggleRenderer(x => x.IsArchive).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
