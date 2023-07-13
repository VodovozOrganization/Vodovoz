using QS.Views.Dialog;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewWidgets.Logistics;

namespace Vodovoz.Views.Logistic
{
	[WindowSize(400, 600)]
	public partial class FastDeliveryTransferDetailsView : DialogViewBase<FastDeliveryTransferDetailsViewModel>
	{
		public FastDeliveryTransferDetailsView(FastDeliveryTransferDetailsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			if(ViewModel == null)
			{
				return;
			}

			var fastDeliveryTransferView = new FastDeliveryTransferView(ViewModel.FastDeliveryTransferViewModel);
			fastDeliveryTransferView.Show();
			yvbox1.PackStart(fastDeliveryTransferView, true, true, 0);
		}
	}
}
