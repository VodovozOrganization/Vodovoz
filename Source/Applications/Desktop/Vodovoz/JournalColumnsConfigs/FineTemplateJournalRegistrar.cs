using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using static Vodovoz.ViewModels.Journals.JournalViewModels.Employees.FineTemplateJournalViewModel;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FineTemplateJournalRegistrar :
		ColumnsConfigRegistrarBase<FineTemplateJournalViewModel, FineTemplateJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FineTemplateJournalNode> config) => config
			.AddColumn("Номер").AddNumericRenderer(x => x.Id)
			.AddColumn("Название").AddTextRenderer(x => x.Title)
			.AddColumn("")
			.Finish();
	}
}
