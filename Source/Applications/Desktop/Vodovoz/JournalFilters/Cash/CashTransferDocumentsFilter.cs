using QS.Views.GtkUI;
using QSOrmProject;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.ViewModels.Cash.TransferDocumentsJournal;

namespace Vodovoz.JournalFilters.Cash
{
	[OrmDefaultIsFiltered(true)]
	public partial class CashTransferDocumentsFilter : FilterViewBase<TransferDocumentsJournalFilterViewModel>
	{
		public CashTransferDocumentsFilter(TransferDocumentsJournalFilterViewModel filterViewModel)
			: base(filterViewModel)
		{
			Build();

			yenumCashTransferDocumentStatus.ItemsEnum = typeof(CashTransferDocumentStatuses);

			yenumCashTransferDocumentStatus.Binding
				.AddBinding(ViewModel, vm => vm.CashTransferDocumentStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();
		}
	}
}
