using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.JournalViewModels.Suppliers;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class RequestsToSuppliersJournalRegistrar : ColumnsConfigRegistrarBase<RequestsToSuppliersJournalViewModel, RequestToSupplierJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<RequestToSupplierJournalNode> config) =>
			config.AddColumn("Номер")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Статус")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Status.GetEnumTitle())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
				.AddColumn("Дата")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Created.ToString("G"))
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Author)
				.AddColumn("")
				.RowCells()
				.AddSetter<CellRendererText>((c, n) =>
				{

					c.ForegroundGdk = n.Status == Domain.Suppliers.RequestStatus.Closed
						? GdkColors.InsensitiveText
						: GdkColors.PrimaryText;
				})
				.Finish();
	}
}
