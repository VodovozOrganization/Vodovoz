using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	public class DriversWarehousesEventsNamesJournalRegistrar :
		ColumnsConfigRegistrarBase<DriversWarehousesEventsNamesJournalViewModel, DriversWarehousesEventsNamesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DriversWarehousesEventsNamesJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Название").AddTextRenderer(x => x.EventName)
				.AddColumn("")
				.Finish();
	}
}
