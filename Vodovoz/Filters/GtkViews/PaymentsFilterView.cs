using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Domain.Payments;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentsFilterView : FilterViewBase<PaymentsJournalFilterViewModel>
	{
		public PaymentsFilterView(PaymentsJournalFilterViewModel viewModel) : base(viewModel)
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
			chkIsManualCreate.Binding
				.AddBinding(ViewModel, vm => vm.IsManualCreate, w => w.Active)
				.InitializeFromSource();
		}
	}
}
