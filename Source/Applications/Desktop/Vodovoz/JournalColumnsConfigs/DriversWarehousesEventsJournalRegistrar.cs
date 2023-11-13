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
				.AddColumn("Тип").AddEnumRenderer(x => x.Type).Editing(false)
				.AddColumn("Широта").AddTextRenderer(x => x.Latitude.HasValue ? x.Latitude.ToString() : "")
				.AddColumn("Долгота").AddTextRenderer(x => x.Longitude.HasValue ? x.Longitude.ToString() : "")
				.AddColumn("В архиве?").AddToggleRenderer(x => x.IsArchive).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
