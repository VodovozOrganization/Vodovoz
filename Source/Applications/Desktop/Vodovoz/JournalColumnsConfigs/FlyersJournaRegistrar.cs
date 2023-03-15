using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Flyers;
using Vodovoz.ViewModels.Journals.JournalViewModels.Flyers;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FlyersJournaRegistrar : ColumnsConfigRegistrarBase<FlyersJournalViewModel, FlyersJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FlyersJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(n => n.Id)
				.AddColumn("Название").AddTextRenderer(n => n.Name)
				.AddColumn("Дата старта").AddTextRenderer(n => n.StartDate.ToShortDateString())
				.AddColumn("Дата окончания").AddTextRenderer(n =>
					n.EndDate.HasValue ? n.EndDate.Value.ToShortDateString() : "")
				.AddColumn("")
				.Finish();
	}
}
