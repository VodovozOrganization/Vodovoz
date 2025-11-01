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

		public int DefaultProfitCategoryId => _settingsController.GetIntValue("default_profit_category_id");
		public int OtherProfitCategoryId => _settingsController.GetIntValue("Payment.OtherProfitCategoryId");
		public int RefundCancelOrderProfitCategoryId => _settingsController.GetIntValue("Payment.RefundCancelOrderProfitCategoryId");
		public DateTime ControlPointStartDate => _settingsController.GetDateTimeValue("Payment.ControlPointStartDate");
	}
}
