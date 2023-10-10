using Gamma.ColumnConfig;
using Vodovoz.JournalColumnsConfigs;
using Vodovoz.ViewModels.Counterparties;

namespace Vodovoz.Counterparties
{
	internal class SubtypesJournalRegistrar : ColumnsConfigRegistrarBase<SubtypesJournalViewModel, SubtypesJournalViewModel.Node>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<SubtypesJournalViewModel.Node> config) =>
			config.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Название").AddTextRenderer(x => x.Title)
				.AddColumn("")
				.Finish();
	}
}
