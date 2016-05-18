using System;
using System.Linq;
using QSOrmProject;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	public static class CurrentUserSettings
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		static UserSettings settings;
		public static UserSettings Settings
		{
			get
			{
				if(settings != null)
					return settings;

				ReloadSettings();

				var map = OrmMain.GetObjectDescription<UserSettings>();
				map.ObjectUpdatedGeneric -= Map_ObjectUpdatedGeneric;
				map.ObjectUpdatedGeneric += Map_ObjectUpdatedGeneric;

				return settings;
			}
		}

		static private void ReloadSettings()
		{
			logger.Info("Обновляем настройки пользователя...");
			using (var Uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				settings = Repository.UserRepository.GetCurrentUserSettings(Uow);

				if (settings == null)
				{
					var user = Repository.UserRepository.GetCurrentUser(Uow);
					settings = new UserSettings(user);
				}
			}

		}

		static void Map_ObjectUpdatedGeneric (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<UserSettings> e)
		{
			if (e.UpdatedSubjects.Any(x => x.Id == Settings.Id))
				ReloadSettings();
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

