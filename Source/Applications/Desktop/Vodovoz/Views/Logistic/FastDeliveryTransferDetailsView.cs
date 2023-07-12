using QS.Views.Dialog;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewWidgets.Logistics;

namespace Vodovoz.Views.Logistic
{
	[WindowSize(800, 600)]
	public partial class FastDeliveryTransferDetailsView : DialogViewBase<FastDeliveryTransferDetailsViewModel>
	{
		public FastDeliveryTransferDetailsView(FastDeliveryTransferDetailsViewModel viewModel) : base(viewModel)
		{
			if(ViewModel == null)
			{
				return;
			}

			this.Build();
		}

		private void ConfigureView()
		{
			var fastDeliveryTransferView = new FastDeliveryTransferView(ViewModel.FastDeliveryTransferViewModel);
			fastDeliveryTransferView.Show();
			yvbox1.PackStart(fastDeliveryTransferView, true, true, 0);
		}
	}
}
