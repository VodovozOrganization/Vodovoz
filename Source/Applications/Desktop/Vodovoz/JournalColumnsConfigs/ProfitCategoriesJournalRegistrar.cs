using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;

namespace Vodovoz.JournalColumnsConfigs
{
	public class ProfitCategoriesJournalRegistrar: ColumnsConfigRegistrarBase<ProfitCategoriesJournalViewModel, ProfitCategoriesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ProfitCategoriesJournalNode> config) =>
			config
				.AddColumn("Код")
					.AddNumericRenderer(x => x.Id)
				.AddColumn("Наименование")
					.AddTextRenderer(x => x.Name)
				.AddColumn("Архивный")
					.AddToggleRenderer(x => x.IsArchive)
					.Editing(false)
				.AddColumn("")
				.Finish();
	}
}
