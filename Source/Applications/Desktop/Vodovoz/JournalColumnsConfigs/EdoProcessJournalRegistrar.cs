using Gamma.ColumnConfig;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.Journals.JournalViewModels.Edo;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class EdoProcessJournalRegistrar : ColumnsConfigRegistrarBase<EdoProcessJournalViewModel, EdoProcessJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EdoProcessJournalNode> config) => config
			.AddColumn("Заказ")
				.HeaderAlignment(0.5f)
				.AddNumericRenderer(node => node.OrderId).Editing(false)
				.XAlign(0.5f)
			.AddColumn("Дата доставки")
				.HeaderAlignment(0.5f)
				.AddDateRenderer(node => node.DeliveryDate).Editable(false)
				.XAlign(0.5f)
			.AddColumn("Статус заказа")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.OrderStatusTitle)
				.XAlign(0.5f)
			.AddColumn("Время заявки")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.CustomerRequestTimeTitle)
				.XAlign(0.5f)
			.AddColumn("Инициатор")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.CustomerRequestSourceTitle)
				.XAlign(0.5f)
			.AddColumn("Задача на\n обработку")
				.HeaderAlignment(0.5f)
				.AddNumericRenderer(node => node.OrderTaskId, new NullableIntToStringConverter()).Editing(false)
				.XAlign(0.5f)
			.AddColumn("Тип задачи")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.OrderTaskTypeTitle)
				.XAlign(0.5f)
			.AddColumn("Статус задачи")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.OrderTaskStatusTitle)
				.XAlign(0.5f)
			.AddColumn("Стадия")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.TaskStage)
				.XAlign(0.5f)
			.AddColumn("Трансфер\n завершен")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.TransfersCompletedTitle)
				.XAlign(0.5f)
			.AddColumn("Проблема\n трансфера")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.TransfersHasProblemsTitle)
				.XAlign(0.5f)
			.AddColumn("Длительность\n трансфера")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.TotalTransferTimeByTransferTasksTitle)
				.XAlign(0.5f)
			.AddColumn("Длительность\n задачи")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.OrderTaskTimeInProgressTitle)
				.XAlign(0.5f)
			.AddColumn("")
			.Finish();
	}
}
