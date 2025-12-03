using Gamma.ColumnConfig;
using Gamma.Utilities;
using Vodovoz.ViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class RecomendationJournalRegistrar : ColumnsConfigRegistrarBase<RecomendationsJournalViewModel, RecomendationJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<RecomendationJournalNode> config) =>
			config.AddColumn("#")
				.AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Название")
				.AddTextRenderer(x => x.Name)
				.AddColumn("Архив")
				.AddToggleRenderer(x => x.IsArchive)
				.Editing(false)
				.AddColumn("Тип помещения")
				.AddTextRenderer(x => x.RoomType.HasValue ? x.RoomType.GetEnumTitle() : "Все")
				.AddColumn("Тип контрагента")
				.AddTextRenderer(x => x.PersonType.HasValue ? x.PersonType.GetEnumTitle() : "Все")
				.Finish();
	}
}
