using Gamma.ColumnConfig;
using Gamma.Utilities;
using Pango;
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
				.AddNumericRenderer(node => node.OrderId, new NullableIntToStringConverter()).Editing(false)
			.AddColumn("Задача")
				.HeaderAlignment(0.5f)
				.AddNumericRenderer(node => node.OrderTaskId, new NullableIntToStringConverter()).Editing(false)
			.AddColumn("Статус задачи")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.OrderTaskStatus.GetEnumTitle())
			.AddColumn("Статус проблемы")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.TaskProblemState.GetEnumTitle())
			.AddColumn("Идентификатор источника")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.ProblemSourceName)
			.AddColumn("Сообщение")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.Message)
				.WrapMode(WrapMode.Word).WrapWidth(300)
			.AddColumn("Дополнительная информация")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.Description)
				.WrapMode(WrapMode.Word).WrapWidth(300)
			.AddColumn("Рекомендация")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.Recomendation)
				.WrapMode(WrapMode.Word).WrapWidth(300)
			.AddColumn("Дата доставки")
				.HeaderAlignment(0.5f)
				.AddDateRenderer(node => node.DeliveryDate).Editable(false)
			.AddColumn("")
			.Finish();
	}
}
