using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;

namespace Vodovoz.JournalColumnsConfigs
{
	public class NotAllocatedCounterpartiesJournalRegistrar : ColumnsConfigRegistrarBase<NotAllocatedCounterpartiesJournalViewModel, NotAllocatedCounterpartiesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<NotAllocatedCounterpartiesJournalNode> config) => config
			.AddColumn("Код").AddNumericRenderer(n => n.Id)
			.AddColumn("ИНН").AddTextRenderer(n => n.Inn)
			.AddColumn("Название").AddTextRenderer(n => n.Name)
			.AddColumn("Категория дохода").AddTextRenderer(n => n.ProfitCategory)
			.AddColumn("")
			.Finish();

	}
}
