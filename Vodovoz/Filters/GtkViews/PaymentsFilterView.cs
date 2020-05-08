using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Domain.Payments;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentsFilterView : FilterViewBase<PaymentsJournalFilterViewModel>
	{
		public PaymentsFilterView(PaymentsJournalFilterViewModel paymentsJournalFilterVM) : base(paymentsJournalFilterVM)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			dateRangeFilter.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			dateRangeFilter.Binding.AddBinding(ViewModel, vm=> vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();
			yenumcomboPaymentState.Binding.AddBinding(ViewModel, vm => vm.PaymentState, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboPaymentState.ItemsEnum = typeof(PaymentState);
			ycheckbtnHideCompleted.Binding.AddBinding(ViewModel, vm => vm.HideCompleted, w => w.Active).InitializeFromSource();
		}
	}
}
