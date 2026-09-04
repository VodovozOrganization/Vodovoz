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
		public int PersonalDiscountReasonId =>  _settingsController.GetIntValue("DiscountReason.PersonalDiscountReasonId");
		public int FirstOnlineOrderDiscountReasonId =>
			_settingsController.GetIntValue("first_online_order_discount_reason_id");
	}
}
