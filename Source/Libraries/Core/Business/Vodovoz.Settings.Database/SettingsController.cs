using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Vodovoz.Settings.Database
{
	public class SettingsController : ISettingsController
	{
		private readonly ILogger<SettingsController> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private static readonly ConcurrentDictionary<string, Setting> _settings = new ConcurrentDictionary<string, Setting>();

		public SettingsController(IUnitOfWorkFactory uowFactory, ILogger<SettingsController> logger)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public bool ContainsSetting(string settingName)
		{
			if(_settings.ContainsKey(settingName))
			{
				return true;
			}

			RefreshSettings();
			return _settings.ContainsKey(settingName);
		}

		public void CreateOrUpdateSetting(string name, string value, TimeSpan? cacheTimeOut = null)
		{
			bool isInsert = false;
			if(_settings.TryGetValue(name, out var oldSetting))
			{
				if(oldSetting.StrValue == value)
				{
					return;
				}
			}
			else
			{
				isInsert = true;
			}
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				if(isInsert)
				{
					var newSetting = new Setting() { Name = name, StrValue = value };

					if(cacheTimeOut.HasValue)
					{
						newSetting.CacheTimeout = cacheTimeOut.Value;
					}

					uow.Save(newSetting);

					_logger.LogDebug("Добавляем новую настройку в базу {Name}='{Value}'", name, value);
				}
				else
				{
					uow.Session.Refresh(oldSetting);

					oldSetting.StrValue = value;

					if(cacheTimeOut.HasValue)
					{
						oldSetting.CacheTimeout = cacheTimeOut.Value;
					}

					uow.Save(oldSetting);

					_logger.LogDebug("Изменяем настройку в базе {Name}='{Value}'", name, value);
				}

				uow.Commit();

				RefreshSettings();
			}

			_logger.LogDebug("Ок");
		}

		public bool GetBoolValue(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new SettingException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetStringValue(settingName);

			if(string.IsNullOrWhiteSpace(value) || !bool.TryParse(value, out bool result))
			{
				throw new SettingException(GetIncorrectSettingMessage(settingName));
			}

			return result;
		}

		public char GetCharValue(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new SettingException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetSettingValue(settingName);

			if(string.IsNullOrWhiteSpace(value) || !char.TryParse(value, out char result))
			{
				throw new SettingException(GetIncorrectSettingMessage(settingName));
			}

			return result;
		}

		public decimal GetDecimalValue(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new SettingException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetSettingValue(settingName);

			if(string.IsNullOrWhiteSpace(value) || !decimal.TryParse(value, out decimal result))
			{
				throw new SettingException(GetIncorrectSettingMessage(settingName));
			}

			return result;
		}

		public int GetIntValue(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new SettingException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetSettingValue(settingName);

			if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
			{
				throw new SettingException(GetIncorrectSettingMessage(settingName));
			}

			return result;
		}

		public string GetStringValue(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new SettingException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetSettingValue(settingName);

			if(string.IsNullOrWhiteSpace(value))
			{
				throw new SettingException(GetIncorrectSettingMessage(settingName));
			}

			return value;
		}

		public DateTime GetDateTimeValue(string settingName, CultureInfo cultureInfo = null)
		{
			if(!ContainsSetting(settingName))
			{
				throw new SettingException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetSettingValue(settingName);

			if(cultureInfo == null)
			{
				cultureInfo = CultureInfo.GetCultureInfo("ru-RU");
			}

			if(string.IsNullOrWhiteSpace(value) || !DateTime.TryParse(value, cultureInfo, DateTimeStyles.None, out DateTime result))
			{
				throw new SettingException(GetIncorrectSettingMessage(settingName));
			}

			return result;
		}

		public T GetValue<T>(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new SettingException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetSettingValue(settingName);
			if(string.IsNullOrWhiteSpace(value))
			{
				throw new SettingException(GetIncorrectSettingMessage(settingName));
			}

			T result;
			try
			{
				var resultAsObject = TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(value);
				if(resultAsObject == null)
				{
					throw new InvalidCastException($"Ошибка при приведении {value} к типу {typeof(T).Name}");
				}
				result = (T)resultAsObject;
			}
			catch(Exception e)
			{
				throw new SettingException(GetIncorrectSettingMessage(settingName), e);
			}

			return result;
		}

		private string GetSettingValue(string settingName)
		{
			if(string.IsNullOrWhiteSpace(settingName))
			{
				throw new ArgumentNullException(nameof(settingName));
			}

			if(!ContainsSetting(settingName))
			{
				RefreshSettings();

				if(!ContainsSetting(settingName))
				{
					throw new SettingException(GetSettingNotFoundMessage(settingName));
				}
			}

			var setting = _settings[settingName];
			if(setting.IsExpired)
			{
				RefreshSettings();
				setting = _settings[settingName];
			}

			if(string.IsNullOrWhiteSpace(setting.StrValue))
			{
				throw new SettingException(GetIncorrectSettingMessage(settingName));
			}

			return setting.StrValue;
		}

		public void RefreshSettings()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				_logger.LogDebug("Обновляем все настройки");
				var settings = uow.Session.QueryOver<Setting>().List();
				var oldSettings = _settings.Values.ToList();

				foreach(var newSetting in settings)
				{
					if(_settings.TryGetValue(newSetting.Name, out var currentSetting))
					{
						var oldSetting = oldSettings.SingleOrDefault(x => x.Name == newSetting.Name);

						if(oldSetting != null)
						{
							oldSettings.Remove(oldSetting);
						}
						
						if(currentSetting.IsExpired)
						{
							currentSetting.CachedTime = DateTime.Now;
							currentSetting.StrValue = newSetting.StrValue;
						}
						else
						{
							continue;
						}
					}
					
					newSetting.CachedTime = DateTime.Now;
					_settings.TryAdd(newSetting.Name, newSetting);
				}

				foreach(var oldSetting in oldSettings)
				{
					_settings.TryRemove(oldSetting.Name, out var deletedSetting);
				}
			}
		}

		private string GetSettingNotFoundMessage(string settingName)
		{
			return $"В базе данных не добавлена настройка ({settingName})";
		}

		private string GetIncorrectSettingMessage(string settingName)
		{
			return $"В базе данных настройка ({settingName}) имеет некорректное значение.";
		}
	}
}
