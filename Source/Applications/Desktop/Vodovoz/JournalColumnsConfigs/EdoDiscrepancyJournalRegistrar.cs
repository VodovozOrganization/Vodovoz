using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.Journals.JournalViewModels.Edo;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class EdoDiscrepancyJournalRegistrar : ColumnsConfigRegistrarBase<EdoDiscrepancyJournalViewModel, EdoDiscrepancyJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EdoDiscrepancyJournalNode> config) => config
			.AddColumn("Заказ")
				.HeaderAlignment(0.5f)
				.AddNumericRenderer(node => node.OrderId).Editing(false)
				.XAlign(0.5f)
			.AddColumn("Дата доставки")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.DeliveryDateTitle)
				.XAlign(0.5f)
			.AddColumn("Статус заказа")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.OrderStatusTitle)
				.XAlign(0.5f)
			.AddColumn("Номер УПД")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.DocumentNumber)
				.XAlign(0.5f)
			.AddColumn("Задача")
				.HeaderAlignment(0.5f)
				.AddNumericRenderer(node => node.TaskId).Editing(false)
				.XAlign(0.5f)
			.AddColumn("Статус задачи")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.EdoTaskStatusTitle)
				.XAlign(0.5f)
			.AddColumn("Статус документа в ДВ")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.UpdInErpStatusTitle)
				.XAlign(0.5f)
			.AddColumn("Статус документа в Такском")
				.HeaderAlignment(0.5f)
				.AddReadOnlyTextRenderer(node => node.UpdInTaxcomStatusTitle)
				.XAlign(0.5f)
			.AddColumn("")
			.Finish();
	}
}
