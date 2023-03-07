using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class RoboatsWaterTypeJournalRegistrar : ColumnsConfigRegistrarBase<RoboatsWaterTypeJournalViewModel, RoboatsWaterTypeJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<RoboatsWaterTypeJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Nomenclature)
				.AddColumn("Готов для Roboats").AddTextRenderer(node => node.ReadyForRoboats ? "Да" : string.Empty)
				.AddColumn("")
				.Finish();
	}
}
