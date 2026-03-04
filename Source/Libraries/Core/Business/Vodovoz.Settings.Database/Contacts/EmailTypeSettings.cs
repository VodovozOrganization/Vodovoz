using Vodovoz.Settings.Contacts;

namespace Vodovoz.Settings.Database.Contacts
{
	public class EmailTypeSettings : IEmailTypeSettings
	{
		private readonly ISettingsController _settingsController;

		public EmailTypeSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new System.ArgumentNullException(nameof(settingsController));
		}

		public int ArchiveId => _settingsController.GetIntValue("email_type_archive_id");
	}
}
