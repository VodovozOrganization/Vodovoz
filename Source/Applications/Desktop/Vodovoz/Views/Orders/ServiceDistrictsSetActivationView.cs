using QS.Views.GtkUI;
using Vodovoz.ViewModels.Logistic;
namespace Vodovoz.Views.Orders
{
	public partial class ServiceDistrictsSetActivationView : TabViewBase<ServiceDistrictsSetActivationViewModel>
	{
		public ServiceDistrictsSetActivationView(ServiceDistrictsSetActivationViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ylabelSelectedDistrictsSetStr.Text = ViewModel.Entity?.Name ?? "";

			ybuttonActivate.BindCommand(ViewModel.ActivateCommand);

			ybuttonActivate.Binding
				.AddFuncBinding(ViewModel, vm => !ViewModel.ActivationInProgress && !ViewModel.WasActivated, w => w.Sensitive)
				.InitializeFromSource();

			ylabelActivationStatus.Binding
				.AddBinding(ViewModel, vm => vm.ActivationStatus, w => w.LabelProp)
				.InitializeFromSource();

			ylabelCurrentDistrictsSetStr.Binding
				.AddBinding(ViewModel, vm => vm.ActiveServiceDistrictsSetName, w => w.LabelProp)
				.InitializeFromSource();

			ylabelSelectedDistrictsSetStr.Binding
				.AddBinding(ViewModel, vm => vm.SelectedServiceDistrictName, w => w.LabelProp)
				.InitializeFromSource();
		}
	}
}
