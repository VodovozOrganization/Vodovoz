using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class RoboatsWaterNomenclatureJournalRegistrar : ColumnsConfigRegistrarBase<RoboatsWaterNomenclatureJournalViewModel, WaterJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<WaterJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Title)
				.AddColumn("")
				.Finish();
	}
}
