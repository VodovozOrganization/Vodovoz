using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewModels.Orders
{
	public class FastDeliveryVerificationDetailsViewModel : WindowDialogViewModelBase
	{
		public FastDeliveryVerificationDetailsViewModel(INavigationManager navigationManager, FastDeliveryVerificationViewModel fastDeliveryVerificationViewModel)
			: base(navigationManager)
		{
			FastDeliveryVerificationViewModel = fastDeliveryVerificationViewModel ?? throw new ArgumentNullException(nameof(fastDeliveryVerificationViewModel));

			WindowPosition = WindowGravity.None;
		}

		public FastDeliveryVerificationViewModel FastDeliveryVerificationViewModel { get; }
	}
}
