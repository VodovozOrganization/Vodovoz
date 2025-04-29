using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Store;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal class RegradingOfGoodsTemplateJournalRegistrar : ColumnsConfigRegistrarBase<RegradingOfGoodsTemplateJournalViewModel, RegradingOfGoodsTemplateJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<RegradingOfGoodsTemplateJournalNode> config) =>
			config.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.Finish();
	}
}
