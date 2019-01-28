using QSOrmProject.Users;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	public static class CurrentUserSettings
	{
		static UserSettingsManager<UserSettings> manager = new UserSettingsManager<UserSettings>();

		static CurrentUserSettings()
		{
			manager.CreateUserSettings = uow => new UserSettings(Repositories.HumanResources.UserRepository.GetCurrentUser(uow));
			manager.LoadUserSettings = Repositories.HumanResources.UserRepository.GetCurrentUserSettings;
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

