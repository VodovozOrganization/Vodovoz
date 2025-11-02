using System;
using Vodovoz.Settings.FastPayments;

namespace Vodovoz.Settings.Database.FastPayments
{
	public class FastPaymentSettings : IFastPaymentSettings
	{
		private readonly ISettingsController _settingsController;

		public FastPaymentSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int GetQRLifetime => _settingsController.GetIntValue("fast_payment_qr_lifetime");
		public int GetPayUrlLifetime => _settingsController.GetIntValue("fast_payment_pay_url_lifetime");
		public int GetOnlinePayByQRLifetime => _settingsController.GetIntValue("fast_payment_online_pay_by_qr_lifetime");
		public int GetDefaultShopId => _settingsController.GetIntValue("default_fast_payment_shop_id");
		public string GetFastPaymentBackUrl => _settingsController.GetStringValue("fast_payment_back_url");
		public string GetFastPaymentApiBaseUrl => _settingsController.GetStringValue("fast_payment_api_base_url");
		public string GetAvangardFastPayBaseUrl => _settingsController.GetStringValue("avangard_fast_pay_base_url");
		public string GetVodovozFastPayBaseUrl => _settingsController.GetStringValue("vodovoz_fast_pay_base_url");
	}
}
