using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.EntityRepositories;
using Vodovoz.Services;

namespace Vodovoz
{
	public static class CurrentUserSettings
	{
		static UserSettingsManager<UserSettings> manager = new UserSettingsManager<UserSettings>();

		static CurrentUserSettings()
		{
			var userRepository = ScopeProvider.Scope.Resolve<IUserRepository>();
			manager.CreateUserSettings = uow => new UserSettings(userRepository.GetCurrentUser(uow));
			manager.LoadUserSettings = userRepository.GetCurrentUserSettings;
		}

		public static UserSettings Settings => manager.Settings;

		public static void SaveSettings()
		{
			manager.SaveSettings();
		}
	}
	
	public class UserSettingsService : IUserSettingsService
	{
		public UserSettings Settings => CurrentUserSettings.Settings;
	}

	public class UserSettingsManager<TUserSettings> where TUserSettings : IDomainObject
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public Func<IUnitOfWork, TUserSettings> LoadUserSettings;
		public Func<IUnitOfWork, TUserSettings> CreateUserSettings;

		public UserSettingsManager()
		{
		}

		IUnitOfWork uow;

		IUnitOfWork UoW
		{
			get
			{
				if(uow == null || !uow.IsAlive)
					uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
				return uow;
			}
		}

		TUserSettings settings;
		public TUserSettings Settings
		{
			get
			{
				if(settings != null && UoW != null && UoW.IsAlive)
					return settings;

				ReloadSettings();

				var map = OrmMain.GetObjectDescription<TUserSettings>();
				if(map != null)
				{
					map.ObjectUpdatedGeneric -= Map_ObjectUpdatedGeneric;
					map.ObjectUpdatedGeneric += Map_ObjectUpdatedGeneric;
				}
				else
					logger.Warn($"Класс {typeof(TUserSettings)} не добавлен в {nameof(OrmMain.AddObjectDescription)}, поэтому подписка на измененя настроек не возможна.");

				return settings;
			}
		}

		private void ReloadSettings()
		{
			logger.Info("Обновляем настройки пользователя...");
			settings = LoadUserSettings(UoW);

			if(settings == null)
			{
				logger.Info("Настроек пользователя нет, создаем новые.");
				settings = CreateUserSettings(UoW);
				SaveSettings();
			}
		}

		void Map_ObjectUpdatedGeneric(object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<TUserSettings> e)
		{
			if(e.UpdatedSubjects.Any(x => x.Id == Settings.Id))
				UoW.Session.Refresh(Settings);
		}

		public void SaveSettings()
		{
			UoW.Save(settings);
			UoW.Commit();
		}

	}
}
