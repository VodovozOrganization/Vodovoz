using Vodovoz.Settings.Contacts;

namespace Vodovoz.Settings.Database.Contacts
{
	public class PhoneTypeSettings : IPhoneTypeSettings
	{
		private readonly ISettingsController _settingsController;

		public PhoneTypeSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new System.ArgumentNullException(nameof(settingsController));
		}

		public int ArchiveId => _settingsController.GetIntValue("phone_type_archive_id");
	}
}
