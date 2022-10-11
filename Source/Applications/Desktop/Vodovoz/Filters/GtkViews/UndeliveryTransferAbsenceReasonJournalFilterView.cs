using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.Filters.GtkViews
{
	public partial class UndeliveryTransferAbsenceReasonJournalFilterView : FilterViewBase<UndeliveryTransferAbsenceReasonJournalFilterViewModel>
	{
		public UndeliveryTransferAbsenceReasonJournalFilterView(UndeliveryTransferAbsenceReasonJournalFilterViewModel viewModel)
			: base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ydateperiodpickerCreateEventDate.Binding.AddBinding(ViewModel, vm => vm.CreateEventDateFrom, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodpickerCreateEventDate.Binding.AddBinding(ViewModel, vm => vm.CreateEventDateTo, w => w.EndDateOrNull).InitializeFromSource();

			ychkIsArchive.Binding.AddBinding(ViewModel, vm => vm.ShowArchive, w => w.Active).InitializeFromSource();
		}
	}
}
