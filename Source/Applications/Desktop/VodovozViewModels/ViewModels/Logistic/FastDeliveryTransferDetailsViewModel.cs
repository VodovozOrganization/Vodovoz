using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class FastDeliveryTransferDetailsViewModel : WindowDialogViewModelBase
	{
		private readonly FastDeliveryTransferViewModel _fastDeliveryTransferViewModel;

		public FastDeliveryTransferDetailsViewModel(INavigationManager navigationManager, FastDeliveryTransferViewModel fastDeliveryTransferViewModel)
			:base(navigationManager)
		{
			_fastDeliveryTransferViewModel = fastDeliveryTransferViewModel ?? throw new ArgumentNullException(nameof(fastDeliveryTransferViewModel));
		}

		public FastDeliveryTransferViewModel FastDeliveryTransferViewModel => _fastDeliveryTransferViewModel;
	}
}
