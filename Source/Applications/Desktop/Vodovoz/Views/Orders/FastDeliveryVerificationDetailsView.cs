using QS.Views.Dialog;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewWidgets.Logistics;

namespace Vodovoz.Views.Orders
{
	[WindowSize(800, 600)]
	public partial class FastDeliveryVerificationDetailsView : DialogViewBase<FastDeliveryVerificationDetailsViewModel>
	{
		private readonly Gdk.Color _colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
		private readonly Gdk.Color _colorLightRed = new Gdk.Color(0xff, 0x66, 0x66);

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
