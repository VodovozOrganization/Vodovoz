using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class CompletedDriversWarehousesEventsJournalRegistrar :
		ColumnsConfigRegistrarBase<CompletedDriversWarehousesEventsJournalViewModel, CompletedDriversWarehousesEventsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<CompletedDriversWarehousesEventsJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Название события").AddTextRenderer(x => x.EventName)
				.AddColumn("Тип").AddEnumRenderer(x => x.EventType).Editing(false)
				.AddColumn("Водитель").AddTextRenderer(x => x.DriverName)
				.AddColumn("Автомобиль").AddTextRenderer(x => x.Car)
				.AddColumn("Время фиксации").AddTextRenderer(x => x.CompletedDate.ToString())
				.AddColumn("Расстояние\nот места сканирования").AddNumericRenderer(x => x.DistanceMetersFromScanningLocation)
				.AddColumn("")
				.Finish();
	}
}
