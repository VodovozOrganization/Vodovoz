using Gamma.ColumnConfig;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class WaterJournalRegistrar : ColumnsConfigRegistrarBase<WaterJournalViewModel, WaterJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<WaterJournalNode> config) =>
			config.AddColumn("Код")
				.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Номенклатура")
				.AddTextRenderer(node => node.Name)
				.Finish();
	}
}
