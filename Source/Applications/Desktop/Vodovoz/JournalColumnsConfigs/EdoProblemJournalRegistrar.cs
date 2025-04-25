using Gamma.ColumnConfig;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.Journals.JournalViewModels.Edo;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class EdoProblemJournalRegistrar : ColumnsConfigRegistrarBase<EdoProblemJournalViewModel, EdoProblemJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EdoProblemJournalNode> config) => config
			.AddColumn("Заказ")
				.HeaderAlignment(0.5f)
				.AddNumericRenderer(node => node.OrderId).Editing(false)
				.XAlign(0.5f)
			.AddColumn("Задача")
				.HeaderAlignment(0.5f)
				.AddNumericRenderer(node => node.OrderTaskId, new NullableIntToStringConverter()).Editing(false)
				.XAlign(0.5f)
			.AddColumn("Статус задачи")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.OrderTaskStatusTitle)
				.XAlign(0.5f)
			.AddColumn("Статус проблемы")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.TaskProblemStateTitle)
				.XAlign(0.5f)
			.AddColumn("Идентификатор источника")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.SourceId)
				.XAlign(0.5f)
			.AddColumn("Сообщение")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.Message)
				.XAlign(0.5f)
			.AddColumn("Дополнительная информация")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.Description)
				.XAlign(0.5f)
			.AddColumn("Рекомендация")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.Recomendation)
				.XAlign(0.5f)
			.AddColumn("Дата доставки")
				.HeaderAlignment(0.5f)
				.AddDateRenderer(node => node.DeliveryDate).Editable(false)
				.XAlign(0.5f)
			.AddColumn("")
			.Finish();
	}
}
