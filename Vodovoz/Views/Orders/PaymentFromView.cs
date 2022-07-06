using QS.Navigation;
using QS.Views;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class PaymentFromView : ViewBase<PaymentFromViewModel>
	{
		public PaymentFromView(PaymentFromViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			btnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, e) => ViewModel.Close(false, CloseSource.Cancel);

			btnSave.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entryName.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entryOrganizationForAvangardPayments.SetEntityAutocompleteSelectorFactory(ViewModel.OrganizationSelectorFactory);
			entryOrganizationForAvangardPayments.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
