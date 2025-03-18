using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Orders;
using Vodovoz.Views.Common;

namespace Vodovoz.Views.Orders
{
	public partial class PaymentFromView : TabViewBase<PaymentFromViewModel>
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
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			var organizationsWidget = new AddOrRemoveIDomainObjectView();
			organizationsWidget.ViewModel = ViewModel.OrganizationsViewModel;
			organizationsWidget.Show();
			tableMain.Attach(organizationsWidget, 0, 1, 4, 5);

			txtViewOrganizationCriterion.Binding
				.AddBinding(ViewModel, vm => vm.OrganizationsCriterion, w => w.Buffer.Text)
				.InitializeFromSource();

			yChkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, wm => wm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
