using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Journals.Nodes.Cash;
using Vodovoz.ViewModels.Journals.Nodes.Employees;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class VatRateJournalRegistars : ColumnsConfigRegistrarBase<VatRateJournalViewModel, VatRateJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<VatRateJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Размер ставки").AddTextRenderer(node => node.VatRateValue == 0 ? "Без НДС" : $"{node.VatRateValue}%")
				.AddColumn("")
				.Finish();
	}
}
