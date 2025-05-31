using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using VodovozInfrastructure.Extensions;

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
				.AddColumn("В архиве")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsArchive.ConvertToYesOrNo())
					.XAlign(0.5f)
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? GdkColors.InsensitiveText : GdkColors.PrimaryText)
				.Finish();
	}
}
