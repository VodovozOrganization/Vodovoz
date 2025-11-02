using System;
using Vodovoz.Settings.User;

namespace Vodovoz.Settings.Database.User
{
	public class UserRoleSettings : IUserRoleSettings
	{
		private readonly ISettingsController _settingsController;

		public UserRoleSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string GetDefaultUserRoleName => _settingsController.GetStringValue("default_user_role_name");
		public string GetDatabaseForNewUser => _settingsController.GetStringValue("database_for_new_user");
	}
}
