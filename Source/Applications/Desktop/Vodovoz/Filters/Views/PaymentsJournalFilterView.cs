using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using Vodovoz.Domain.Payments;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.Filters.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentsJournalFilterView : FilterViewBase<PaymentsJournalFilterViewModel>
	{
		public PaymentsJournalFilterView(PaymentsJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		void Configure()
		{
			dateRangeFilter.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm=> vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			yenumcomboPaymentState.Binding
				.AddBinding(ViewModel, vm => vm.PaymentState, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			yenumcomboPaymentState.ItemsEnum = typeof(PaymentState);
			ycheckbtnHideCompleted.Binding
				.AddBinding(ViewModel, vm => vm.HideCompleted, w => w.Active)
				.InitializeFromSource();
			chkHideAllocatedPayments.Binding
				.AddBinding(ViewModel, vm => vm.HideAllocatedPayments, w => w.Active)
				.InitializeFromSource();
			chkIsManualCreate.Binding
				.AddBinding(ViewModel, vm => vm.IsManuallyCreated, w => w.Active)
				.InitializeFromSource();
			chkPaymentsWithoutCounterparty.Binding
				.AddBinding(ViewModel, vm => vm.HidePaymentsWithoutCounterparty, w => w.Active)
				.InitializeFromSource();
			chkHideCancelledPayments.Binding
				.AddBinding(ViewModel, vm => vm.HideCancelledPayments, w => w.Active)
				.InitializeFromSource();

			ConfigureEntry();
		}

		private void ConfigureEntry()
		{
			var builder = new LegacyEEVMBuilderFactory<PaymentsJournalFilterViewModel>(
				ViewModel.JournalTab, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.Scope);
			counterpartyEntry.ViewModel = builder.ForProperty(x => x.Counterparty)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();
		}
	}
}
