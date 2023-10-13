using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	public class DriversWarehousesEventsJournalRegistrar :
		ColumnsConfigRegistrarBase<DriversWarehousesEventsJournalViewModel, DriversWarehousesEventsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DriversWarehousesEventsJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Название").AddTextRenderer(x => x.EventName)
				.AddColumn("Тип").AddComboRenderer(x => x.Type).Editing(false)
				.AddColumn("В архиве?").AddToggleRenderer(x => x.IsArchive).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
