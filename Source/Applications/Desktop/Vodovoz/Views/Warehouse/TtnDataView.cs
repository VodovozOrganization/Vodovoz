using QS.Views.Dialog;
using Vodovoz.ViewModels.ViewModels.Warehouses;

namespace Vodovoz.Views.Warehouse
{
	public partial class TtnDataView : DialogViewBase<TtnDataViewModel>
	{
		public TtnDataView(TtnDataViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			entryCargoSender.ViewModel = ViewModel.CargoSenderViewModel;

			entryCargoReceiver.ViewModel = ViewModel.CargoReceiverViewModel;

			entryPayer.ViewModel = ViewModel.PayerViewModel;

			entryCar.ViewModel = ViewModel.MovementWagonViewModel;

			entryTrailer.ViewModel = ViewModel.TrailerViewModel;

			entryDriver.ViewModel = ViewModel.DriverViewModel;

			buttonAccept.BindCommand(ViewModel.AcceptCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}
