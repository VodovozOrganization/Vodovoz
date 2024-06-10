using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Pacs;
using Vodovoz.ViewModels.Journals.JournalViewModels.Pacs;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class OperatorsJournalRegistrar : ColumnsConfigRegistrarBase<OperatorsJournalViewModel, OperatorNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OperatorNode> config) =>
			config
				.AddColumn("#").AddNumericRenderer(node => node.Id)
				.AddColumn("Оператор").AddTextRenderer(node => node.Title)
				.AddColumn("График").AddTextRenderer(node => node.WorkshiftName)
				.AddColumn("Включен").AddToggleRenderer(node => node.PacsEnabled).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
