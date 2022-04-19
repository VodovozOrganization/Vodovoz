using System;
using QS.ViewModels.Dialog;
using QS.Navigation;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class FastDeliveryVerificationDetailsViewModel : WindowDialogViewModelBase
	{
		public FastDeliveryVerificationDetailsViewModel(
			INavigationManager navigationManager
		) : base(navigationManager)
		{
		}
	}
}
