using QS.Views.Dialog;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewWidgets.Logistics;

namespace Vodovoz.Views.Orders
{
	[WindowSize(800, 600)]
	public partial class FastDeliveryVerificationDetailsView : DialogViewBase<FastDeliveryVerificationDetailsViewModel>
	{
		public FastDeliveryVerificationDetailsView(FastDeliveryVerificationDetailsViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			var fastDeliveryVerificationView = new FastDeliveryVerificationView(ViewModel.FastDeliveryVerificationViewModel);
			fastDeliveryVerificationView.Show();
			vboxMain.PackStart(fastDeliveryVerificationView, true, true, 0);
		}
	}
}
