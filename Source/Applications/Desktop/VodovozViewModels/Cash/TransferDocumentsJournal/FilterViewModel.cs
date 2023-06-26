using QS.Project.Filter;
using QS.Project.Journal;
using Vodovoz.Domain.Cash.CashTransfer;

namespace Vodovoz.ViewModels.Cash.TransferDocumentsJournal
{
	public class FilterViewModel : FilterViewModelBase<FilterViewModel>, IJournalFilterViewModel
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
