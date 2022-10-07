using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewWidgets.Logistics;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FastDeliveryAvailabilityHistoryView : TabViewBase<FastDeliveryAvailabilityHistoryViewModel>
	{
		public FastDeliveryAvailabilityHistoryView(FastDeliveryAvailabilityHistoryViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			var fastDeliveryVerificationView = new FastDeliveryVerificationView(ViewModel.FastDeliveryVerificationViewModel);
			fastDeliveryVerificationView.Show();
			yhboxVerificationWidget.PackStart(fastDeliveryVerificationView, true, true, 0);

			buttonClose.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
		}
	}
}
