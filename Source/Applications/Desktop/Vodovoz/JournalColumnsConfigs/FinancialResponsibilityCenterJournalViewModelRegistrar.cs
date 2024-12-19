using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FinancialResponsibilityCenterJournalViewModelRegistrar : ColumnsConfigRegistrarBase<FinancialResponsibilityCenterJournalViewModel, FinancialResponsibilityCenterNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FinancialResponsibilityCenterNode> config) =>
			config
				.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("")
				.Finish();
	}
}
