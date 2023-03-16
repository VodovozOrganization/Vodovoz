using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class UndeliveryTransferAbsenceReasonJournalRegistrar : ColumnsConfigRegistrarBase<UndeliveryTransferAbsenceReasonJournalViewModel, UndeliveryTransferAbsenceReasonJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<UndeliveryTransferAbsenceReasonJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id.ToString())
				.AddColumn("Причина отсутствия переноса").AddTextRenderer(node => node.Name)
				.AddColumn("Дата создания").AddTextRenderer(node => node.CreateDate.ToShortDateString())
				.Finish();
	}
}
