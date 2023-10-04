using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	public class CompletedDriversWarehousesEventsJournalRegistrar :
		ColumnsConfigRegistrarBase<CompletedDriversWarehousesEventsJournalViewModel, CompletedDriversWarehousesEventsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<CompletedDriversWarehousesEventsJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Название события").AddTextRenderer(x => x.EventName)
				.AddColumn("Тип").AddComboRenderer(x => x.Type).Editing(false)
				.AddColumn("Водитель").AddTextRenderer(x => x.DriverName)
				.AddColumn("Автомобиль").AddTextRenderer(x => x.Car)
				.AddColumn("Расстояние между точками").AddNumericRenderer(x => x.DistanceMetersBetweenPoints)
				.AddColumn("")
				.Finish();
	}
}
