using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.EntityRepositories;
using VodovozBusiness.Services.Users;

namespace Vodovoz.Cores
{
	public class UserSettingsManager : IUserSettingsManager
	{
		private readonly ILogger<UserSettingsManager> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IUserRepository _userRepository;
		private UserSettings _settings;

		public UserSettingsManager(
			ILogger<UserSettingsManager> logger,
			IUnitOfWorkFactory uowFactory,
			IUserRepository userRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

			Initialize();
		}

		private void Initialize()
		{
			LoadSettings();
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<UserSettings>(OnUserSettingsChanged);
		}

		public UserSettings Settings => _settings;
		
		public void SaveSettings()
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Сохранение настроек пользователя"))
			{
				SaveSettings(uow);
			}
		}
		
		private void LoadSettings()
		{
			_logger.LogInformation("Обновляем настройки пользователя...");

			using(var uow = _uowFactory.CreateWithoutRoot("Загрузка настроек пользователя"))
			{
				_settings = _userRepository.GetCurrentUserSettings(uow);

				if(_settings is null)
				{
					_logger.LogInformation("Настроек пользователя нет, создаем новые...");
					_settings = new UserSettings(_userRepository.GetCurrentUser(uow));
					
					SaveSettings(uow);
				}
			}
		}
		
		private void OnUserSettingsChanged(EntityChangeEvent[] changeEvents)
		{
			var changedSettings = changeEvents
				.Select(x => x.Entity)
				.OfType<UserSettings>()
				.FirstOrDefault(x => x.Id == _settings.Id);

			if(changedSettings != null)
			{
				LoadSettings();
			}
		}

		private void SaveSettings(IUnitOfWork uow)
		{
			uow.Save(_settings);
			uow.Commit();
		}

		public void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
		}
	}
}
