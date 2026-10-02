using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Receipts;
using Vodovoz.ViewModels.Journals.JournalViewModels.Receipts;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ReceiptCorrectionExplanatoryNoteJournalRegistrar : ColumnsConfigRegistrarBase<ReceiptCorrectionExplanatoryNoteJournalViewModel, ReceiptCorrectionExplanatoryNoteJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ReceiptCorrectionExplanatoryNoteJournalNode> config) => config
			.AddColumn("№").AddNumericRenderer(x => x.RowNumber).Editing(false)
			.AddColumn("Дата формирования").AddDateRenderer(x => x.CreatedDate).Editable(false)
			.AddColumn("Организация").AddReadOnlyTextRenderer(x => x.OrganizationName)
			.AddColumn("Заказ").AddNumericRenderer(x => x.OrderId).Editing(false)
			.AddColumn("Чек").AddReadOnlyTextRenderer(x => x.FiscalDocumentNumber)
			.AddColumn("Основание").AddReadOnlyTextRenderer(x => x.BasisTitle)
			.AddColumn("")
			.Finish();
	}
}
