using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewModels.Orders
{
	public class FastDeliveryVerificationDetailsViewModel : WindowDialogViewModelBase
	{
		public FastDeliveryVerificationDetailsViewModel(INavigationManager navigationManager, FastDeliveryVerificationDTO fastDeliveryVerificationDTO)
			: base(navigationManager)
		{
			FastDeliveryVerificationViewModel = new FastDeliveryVerificationViewModel(
					fastDeliveryVerificationDTO ?? throw new ArgumentNullException(nameof(fastDeliveryVerificationDTO)));

			WindowPosition = WindowGravity.None;
		}

		public FastDeliveryVerificationViewModel FastDeliveryVerificationViewModel { get; }
	}
}
