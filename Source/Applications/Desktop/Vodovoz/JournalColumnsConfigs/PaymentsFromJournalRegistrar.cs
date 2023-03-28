using Gamma.ColumnConfig;
using Gtk;
using Gdk;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PaymentsFromJournalRegistrar : ColumnsConfigRegistrarBase<PaymentsFromJournalViewModel, PaymentFromJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorDarkGray = new Color(0x80, 0x80, 0x80);

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
				.AddColumn("В архиве")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsArchive ? "Да" : "Нет")
					.XAlign(0.5f)
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGray : _colorBlack)
				.Finish();
	}
}
