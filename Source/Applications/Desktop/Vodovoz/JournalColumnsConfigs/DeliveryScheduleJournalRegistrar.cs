using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class DeliveryScheduleJournalRegistrar : ColumnsConfigRegistrarBase<DeliveryScheduleJournalViewModel, DeliveryScheduleJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DeliveryScheduleJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Время доставки").AddTextRenderer(node => node.DeliveryTime)
				.AddColumn("Архивный?").AddTextRenderer(node => node.IsArchive ? "Да" : string.Empty)
				.AddColumn("Готов для Roboats").AddTextRenderer(node => node.ReadyForRoboats ? "Да" : string.Empty)
				.AddColumn("")
				.Finish();
	}
}
