using System;
using Vodovoz.Services;

namespace Vodovoz.Settings.Database.Payments
{
	public class PaymentSettings : IPaymentSettings
	{
		private readonly ISettingsController _settingsController;

		public PaymentSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int DefaultProfitCategory => _settingsController.GetIntValue("default_profit_category_id");
	}
}
