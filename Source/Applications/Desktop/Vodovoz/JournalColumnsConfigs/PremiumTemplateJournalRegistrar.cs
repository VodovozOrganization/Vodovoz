using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PremiumTemplateJournalRegistrar : ColumnsConfigRegistrarBase<PremiumTemplateJournalViewModel, PremiumTemplateJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<PremiumTemplateJournalNode> config) =>
			config.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Шаблон комментария").AddTextRenderer(node => node.Reason)
				.AddColumn("Сумма премии").AddNumericRenderer(node => node.PremiumMoney)
				.Finish();
	}
}
