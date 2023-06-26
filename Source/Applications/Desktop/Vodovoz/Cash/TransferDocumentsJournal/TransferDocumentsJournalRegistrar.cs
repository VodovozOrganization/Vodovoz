using Gamma.ColumnConfig;
using Vodovoz.JournalColumnsConfigs;
using Vodovoz.Representations;
using Vodovoz.ViewModels.Cash.TransferDocumentsJournal;
using Gamma.Utilities;
using QS.Utilities;

namespace Vodovoz.Cash.TransferDocumentsJournal
{
	internal sealed class TransferDocumentsJournalRegistrar : ColumnsConfigRegistrarBase<TransferDocumentsJournalViewModel, DocumentNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DocumentNode> config) =>
			config.AddColumn("№").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Тип").AddTextRenderer(node => node.Name)
				.AddColumn("Дата").AddTextRenderer(node => node.CreationDate.ToShortDateString())
				.AddColumn("Автор").AddTextRenderer(node => node.AuthorShortFullName)
				.AddColumn("Статус").AddTextRenderer(node => node.Status.GetEnumTitle())
				.AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.TransferedSum))

				.AddColumn("Отправлено из").AddTextRenderer(node => node.SubdivisionFrom)
				.AddColumn("Время отпр.").AddTextRenderer(node => node.SendTime.HasValue ? node.SendTime.Value.ToShortDateString() : "")
				.AddColumn("Отправил").AddTextRenderer(node => node.CashierSenderShortFullName)

				.AddColumn("Отправлено в").AddTextRenderer(node => node.SubdivisionTo)
				.AddColumn("Время принятия").AddTextRenderer(node => node.ReceiveTime.HasValue ? node.ReceiveTime.Value.ToShortDateString() : "")
				.AddColumn("Принял").AddTextRenderer(node => node.CashierReceiverShortFullName)

				.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
				.Finish();
	}
}
