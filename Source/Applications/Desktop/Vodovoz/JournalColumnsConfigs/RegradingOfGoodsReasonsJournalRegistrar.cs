using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class RegradingOfGoodsReasonsJournalRegistrar : ColumnsConfigRegistrarBase<RegradingOfGoodsReasonsJournalViewModel, RegradingOfGoodsReasonsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<RegradingOfGoodsReasonsJournalNode> config) =>
			config.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.Finish();
	}
}
