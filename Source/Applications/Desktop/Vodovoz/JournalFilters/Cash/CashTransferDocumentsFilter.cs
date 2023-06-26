using QS.Views.GtkUI;
using QSOrmProject;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.ViewModels.Cash.TransferDocumentsJournal;

namespace Vodovoz.JournalFilters.Cash
{
	[OrmDefaultIsFiltered(true)]
	public partial class CashTransferDocumentsFilter : FilterViewBase<FilterViewModel>
	{
		public CashTransferDocumentsFilter(FilterViewModel filterViewModel)
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
