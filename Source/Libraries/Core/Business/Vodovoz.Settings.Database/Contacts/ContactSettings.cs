using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Contacts;

namespace Vodovoz.Settings.Database.Contacts
{
	public class ContactSettings : IContactSettings
	{
		private readonly ISettingsController _settingsController;

		public ContactSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int MinSavePhoneLength => _settingsController.GetValue<int>("MinSavePhoneLength");
		public string DefaultCityCode => _settingsController.GetValue<string>("default_city_code");
	}
}
