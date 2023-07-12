using QS.Project.Filter;
using QS.Project.Journal;
using Vodovoz.Domain.Cash.CashTransfer;

namespace Vodovoz.ViewModels.Cash.Transfer.Journal
{
	public class TransferDocumentsJournalFilterViewModel : FilterViewModelBase<TransferDocumentsJournalFilterViewModel>, IJournalFilterViewModel
	{
		private CashTransferDocumentStatuses? _cashTransferDocumentStatus;

		public CashTransferDocumentStatuses? CashTransferDocumentStatus
		{
			get => _cashTransferDocumentStatus;
			set => UpdateFilterField(ref _cashTransferDocumentStatus, value);
		}

		public bool IsShow { get; set; }
	}
}
