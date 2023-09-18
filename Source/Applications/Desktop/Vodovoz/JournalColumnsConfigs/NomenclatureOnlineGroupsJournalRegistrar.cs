using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	public class NomenclatureOnlineGroupsJournalRegistrar
		: ColumnsConfigRegistrarBase<NomenclatureOnlineGroupsJournalViewModel, NomenclatureOnlineGroupsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<NomenclatureOnlineGroupsJournalNode> config)
		{
			return config
				.AddColumn("Код")
				.AddNumericRenderer(n => n.Id)
				.AddColumn("Название")
				.AddTextRenderer(n => n.Name)
				.AddColumn("Онлайн типы")
				.AddTextRenderer(n => n.OnlineCategories)
				.Finish();
		}
	}
}
