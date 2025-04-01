using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalViewModels.Edo;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class EdoProcessJournalRegistrar : ColumnsConfigRegistrarBase<EdoProcessJournalViewModel, EdoProcessJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EdoProcessJournalNode> config) => config
			.AddColumn("Заказ")
				.AddNumericRenderer(node => node.OrderId).Editing(false)
			.AddColumn("Дата доставки")
				.AddDateRenderer(node => node.DeliveryDate).Editable(false)
			.AddColumn("Время заявки")
				.AddDateRenderer(node => node.CustomerRequestTime).Editable(false)
			.AddColumn("Источник")
				.AddReadOnlyTextRenderer(node => node.CustomerRequestSourceTitle)
			.AddColumn("Задача")
				.AddNumericRenderer(node => node.OrderTaskId).Editing(false)
			.AddColumn("Тип")
				.AddReadOnlyTextRenderer(node => node.OrderTaskTypeTitle)
			.AddColumn("Статус")
				.AddReadOnlyTextRenderer(node => node.OrderTaskStatusTitle)
			.AddColumn("Стадия")
				.AddReadOnlyTextRenderer(node => node.TaskStage)
			.AddColumn("Трансферы")
				.AddReadOnlyTextRenderer(node => node.TransfersCompletedTitle)
			.AddColumn("Проблема трансфера")
				.AddToggleRenderer(node => node.TransfersHasProblems).Editing(false)
			.AddColumn("Длительность трансфера")
				.AddReadOnlyTextRenderer(node => node.TotalTransferTimeByTransferTasksTitle)
			.AddColumn("Длительность задачи")
				.AddReadOnlyTextRenderer(node => node.OrderTaskTimeInProgressTitle)
			.AddColumn("")
			.Finish();
	}
}
