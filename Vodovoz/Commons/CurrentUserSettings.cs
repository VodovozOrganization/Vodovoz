using System;
using QSOrmProject;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	public static class CurrentUserSettings
	{
		static UserSettings settings;
		public static UserSettings Settings
		{
			get
			{
				if(settings != null)
					return settings;

				using (var Uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					settings = Repository.UserRepository.GetCurrentUserSettings(Uow);

					if (settings == null)
					{
						var user = Repository.UserRepository.GetCurrentUser(Uow);
						settings = new UserSettings(user);
					}
				}

				return settings;
			}
		}

		public static void SaveSettings()
		{
			using (var Uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				Uow.Save(settings);
				Uow.Commit();
			}
		}
	}
}

