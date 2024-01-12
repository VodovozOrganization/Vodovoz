using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class NomenclatureOnlineCategoriesJournalRegistrar
		: ColumnsConfigRegistrarBase<NomenclatureOnlineCategoriesJournalViewModel, NomenclatureOnlineCategoriesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<NomenclatureOnlineCategoriesJournalNode> config)
		{
			return config
				.AddColumn("Код")
				.AddNumericRenderer(n => n.Id)
				.AddColumn("Название")
				.AddTextRenderer(n => n.Name)
				.AddColumn("Онлайн группа")
				.AddTextRenderer(n => n.OnlineGroup)
				.Finish();
		}
	}
}
