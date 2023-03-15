using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PaymentsFromJournalRegistrar : ColumnsConfigRegistrarBase<PaymentsFromJournalViewModel, PaymentFromJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<PaymentFromJournalNode> config) =>
			config.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Id)
					.XAlign(0.5f)
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Name)
					.XAlign(0.5f)
				.AddColumn("Организация\nдля платежей Авангарда")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.OrganizationName)
					.XAlign(0.5f)
				.Finish();
	}
}
