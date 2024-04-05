using System;
using Vodovoz.Settings.Contacts;

namespace Vodovoz.Settings.Database.Contacts
{
	public class PhoneSettings : IPhoneSettings
	{
		private readonly ISettingsController _settingsController;

		public PhoneSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string CourierDispatcherPhone => _settingsController.GetValue<string>(nameof(CourierDispatcherPhone));
	}
}
