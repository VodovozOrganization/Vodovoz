using QS.Navigation;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Payments;

namespace Vodovoz.Views.Payments
{
	public partial class AutomaticallyAllocationBalanceWindowView : DialogViewBase<AutomaticallyAllocationBalanceWindowViewModel>
	{
		public AutomaticallyAllocationBalanceWindowView(AutomaticallyAllocationBalanceWindowViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnAllocateByCurrentCounterparty.Clicked += (sender, args) => ViewModel.AllocateByCurrentCounterpartyCommand.Execute();
			btnAllocateByCurrentCounterparty.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanAllocateByCurrentCounterparty, w => w.Sensitive)
				.InitializeFromSource();

			btnAllocateByAllCounterpartiesWithPositiveBalance.Clicked +=
				(sender, args) => ViewModel.AllocateByAllCounterpartiesWithPositiveBalanceCommand.Execute();

			btnAllocateByAllCounterpartiesWithPositiveBalance.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsAllocationState, w => w.Sensitive)
				.InitializeFromSource();

			btnCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			btnCancel.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsAllocationState, w => w.Sensitive)
				.InitializeFromSource();

			chkAllocateCompletedPayments.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsAllocationState, w => w.Sensitive)
				.AddBinding(vm => vm.AllocateCompletedPayments, w => w.Active)
				.InitializeFromSource();

			ViewModel.ProgressBarDisplayable = progresswidget;
		}
	}
}
