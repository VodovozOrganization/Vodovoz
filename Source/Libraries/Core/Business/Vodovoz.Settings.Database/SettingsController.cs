using Microsoft.Extensions.Logging;
using NHibernate.Persister.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
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

		public void CreateOrUpdateSetting(string name, string value)
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
				var settingPersister = (AbstractEntityPersister)uow.Session.SessionFactory.GetClassMetadata(typeof(Setting));
				var tableName = settingPersister.TableName;
				var nameColumnName = settingPersister.GetPropertyColumnNames(nameof(Setting.Name)).First();
				var strValueColumnName = settingPersister.GetPropertyColumnNames(nameof(Setting.StrValue)).First();

				string sql;
				if(isInsert)
				{
					sql = $"INSERT INTO {tableName} ({nameColumnName}, {strValueColumnName}) VALUES ('{name}', '{value}')";
					_logger.LogDebug("Добавляем новую настройку в базу {Name}='{Value}'", name, value);
				}
				else
				{
					sql = $"UPDATE {tableName} SET {strValueColumnName} = '{value}' WHERE {nameColumnName} = '{name}'";
					_logger.LogDebug("Изменяем настройку в базе {Name}='{Value}'", name, value);
				}
				uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
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
				_settings.Clear();

				foreach(var setting in settings)
				{
					if(_settings.ContainsKey(setting.Name))
					{
						continue;
					}

					setting.CachedTime = DateTime.Now;
					_settings.TryAdd(setting.Name, setting);
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
