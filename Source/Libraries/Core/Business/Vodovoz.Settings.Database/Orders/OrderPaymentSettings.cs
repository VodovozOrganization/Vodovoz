using System;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Settings.Database.Orders
{
	public class OrderPaymentSettings : IOrderPaymentSettings
	{
		private readonly ISettingsController _settingsController;

		public OrderPaymentSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int DefaultSelfDeliveryPaymentFromId => _settingsController.GetIntValue("default_selfdelivery_paymentFrom_id");
	}
}
