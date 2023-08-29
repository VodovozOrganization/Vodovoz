using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class MobileAppNomenclatureOnlineCatalogsJournalRegistrar :
		ColumnsConfigRegistrarBase<MobileAppNomenclatureOnlineCatalogsJournalViewModel, NomenclatureOnlineCatalogsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<NomenclatureOnlineCatalogsJournalNode> config) =>
			config
				.AddColumn("Код")
					.AddNumericRenderer(node => node.Id)
				.AddColumn("Каталог")
					.AddTextRenderer(node => node.Name)
				.AddColumn("")
				.Finish();
	}
}
