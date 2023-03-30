using System;
using Vodovoz.Settings.CashReceipt;

namespace Vodovoz.Settings.Database.CashReceipt
{
	public class CashReceiptSettings : ICashReceiptSettings
	{
		private readonly ISettingsController _settingsController;

		public CashReceiptSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string CashReceiptApiUrl => _settingsController.GetStringValue("CashReceiptApiUrl");

		public string CashReceiptApiKey => _settingsController.GetStringValue("CashReceiptApiKey");
	}
}
