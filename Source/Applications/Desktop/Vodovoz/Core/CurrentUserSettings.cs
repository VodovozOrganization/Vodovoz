using QSOrmProject.Users;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;

namespace Vodovoz
{
	public static class CurrentUserSettings
	{
		static UserSettingsManager<UserSettings> manager = new UserSettingsManager<UserSettings>();

		static CurrentUserSettings()
		{
			var userRepository = new UserRepository();
			manager.CreateUserSettings = uow => new UserSettings(userRepository.GetCurrentUser(uow));
			manager.LoadUserSettings = userRepository.GetCurrentUserSettings;
		}

		public static UserSettings Settings
		{
			get
			{
				return manager.Settings;
			}
		}

		public static void SaveSettings()
		{
			manager.SaveSettings();
		}
	}
}

