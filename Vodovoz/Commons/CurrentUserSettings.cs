using System;
using System.Linq;
using QSOrmProject;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	public static class CurrentUserSettings
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		static IUnitOfWork uow;

		static IUnitOfWork UoW{
			get{
				if (uow == null || !uow.IsAlive)
					uow = UnitOfWorkFactory.CreateWithoutRoot();
				return uow;
			}
		}

		static UserSettings settings;
		public static UserSettings Settings
		{
			get
			{
				if(settings != null && UoW != null && UoW.IsAlive)
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
			settings = Repository.UserRepository.GetCurrentUserSettings(UoW);

			if (settings == null)
			{
				logger.Info("Настроек пользователя нет, создаем новые.");
				var user = Repository.UserRepository.GetCurrentUser(UoW);
				settings = new UserSettings(user);
				SaveSettings();
			}
		}

		static void Map_ObjectUpdatedGeneric (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<UserSettings> e)
		{
			if (e.UpdatedSubjects.Any(x => x.Id == Settings.Id))
				UoW.Session.Refresh(Settings);
		}

		public static void SaveSettings()
		{
			UoW.Save(settings);
			UoW.Commit();
		}
	}
}

