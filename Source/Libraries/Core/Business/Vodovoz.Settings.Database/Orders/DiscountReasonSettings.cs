using System;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Settings.Database.Orders
{
	public class DiscountReasonSettings : IDiscountReasonSettings
	{
		private readonly ISettingsController _settingsController;

		public DiscountReasonSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}
		
		public int GetSelfDeliveryDiscountReasonId => _settingsController.GetIntValue("DiscountReason.SelfDeliveryDiscountReasonId");
	}
}
