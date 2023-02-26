using NHibernate.Persister.Entity;
using NLog;
using QS.DomainModel.UoW;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;

namespace Vodovoz.Settings.Database
{
	public class SettingsController : ISettingsController
	{
		private readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _uowFactory;
		private static readonly ConcurrentDictionary<string, Setting> _settings = new ConcurrentDictionary<string, Setting>();

		public SettingsController(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public bool ContainsSetting(string settingName)
		{
			if(_settings.ContainsKey(settingName))
			{
				return true;
			}

			RefreshSetting(settingName);
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
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var settingPersister = (AbstractEntityPersister)uow.Session.SessionFactory.GetClassMetadata(typeof(Setting));
				var tableName = settingPersister.TableName;
				var nameColumnName = settingPersister.GetPropertyColumnNames(nameof(Setting.Name)).First();
				var strValueColumnName = settingPersister.GetPropertyColumnNames(nameof(Setting.StrValue)).First();

				string sql;
				if(isInsert)
				{
					sql = $"INSERT INTO {tableName} ({nameColumnName}, {strValueColumnName}) VALUES ('{name}', '{value}')";
					_logger.Debug($"Добавляем новую настройку в базу {name}='{value}'");
				}
				else
				{
					sql = $"UPDATE {tableName} SET {strValueColumnName} = '{value}' WHERE {nameColumnName} = '{name}'";
					_logger.Debug($"Изменяем настройку в базе {name}='{value}'");
				}
				uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
				RefreshSetting(name);
			}

			_logger.Debug("Ок");
		}

		private void RefreshSetting(string settingName)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				Setting setting = uow.Session.QueryOver<Setting>()
					.Where(x => x.Name == settingName)
					.List<Setting>().First();

				if(setting == null)
				{
					return;
				}

				setting.CachedTime = DateTime.Now;

				if(_settings.ContainsKey(settingName))
				{
					_settings[settingName] = setting;
					return;
				}

				_settings.TryAdd(setting.Name, setting);
			}
		}

		public bool GetBoolValue(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new InvalidProgramException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetStringValue(settingName);

			if(string.IsNullOrWhiteSpace(value) || !bool.TryParse(value, out bool result))
			{
				throw new InvalidProgramException(GetIncorrectSettingMessage(settingName));
			}

			return result;
		}

		public char GetCharValue(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new InvalidProgramException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetParameterValue(settingName);

			if(string.IsNullOrWhiteSpace(value) || !char.TryParse(value, out char result))
			{
				throw new InvalidProgramException(GetIncorrectSettingMessage(settingName));
			}

			return result;
		}

		public decimal GetDecimalValue(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new InvalidProgramException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetParameterValue(settingName);

			if(string.IsNullOrWhiteSpace(value) || !decimal.TryParse(value, out decimal result))
			{
				throw new InvalidProgramException(GetIncorrectSettingMessage(settingName));
			}

			return result;
		}

		public int GetIntValue(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new InvalidProgramException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetParameterValue(settingName);

			if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
			{
				throw new InvalidProgramException(GetIncorrectSettingMessage(settingName));
			}

			return result;
		}

		public string GetStringValue(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new InvalidProgramException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetParameterValue(settingName);

			if(string.IsNullOrWhiteSpace(value))
			{
				throw new InvalidProgramException(GetIncorrectSettingMessage(settingName));
			}

			return value;
		}

		public T GetValue<T>(string settingName)
		{
			if(!ContainsSetting(settingName))
			{
				throw new InvalidProgramException(GetSettingNotFoundMessage(settingName));
			}

			string value = GetParameterValue(settingName);
			if(string.IsNullOrWhiteSpace(value))
			{
				throw new InvalidProgramException(GetIncorrectSettingMessage(settingName));
			}

			T result;
			try
			{
				var resultAsObject = TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(value);
				if(resultAsObject == null)
				{
					throw new InvalidOperationException("Ошибка при приведении типа");
				}
				result = (T)resultAsObject;
			}
			catch(Exception e)
			{
				throw new InvalidProgramException(GetIncorrectSettingMessage(settingName), e);
			}

			return result;
		}

		private string GetParameterValue(string settingName)
		{
			if(string.IsNullOrWhiteSpace(settingName))
			{
				throw new ArgumentNullException(nameof(settingName));
			}

			if(!ContainsSetting(settingName))
			{
				throw new InvalidProgramException(GetSettingNotFoundMessage(settingName));
			}

			var parameter = _settings[settingName];
			if(parameter.IsExpired)
			{
				RefreshSetting(settingName);
				parameter = _settings[settingName];
			}

			if(String.IsNullOrWhiteSpace(parameter.StrValue))
			{
				throw new InvalidProgramException(GetIncorrectSettingMessage(settingName));
			}

			return parameter.StrValue;
		}

		public void RefreshSettings()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var allParameters = uow.Session.QueryOver<Setting>().List();
				_settings.Clear();

				foreach(var parameter in allParameters)
				{
					if(_settings.ContainsKey(parameter.Name))
					{
						continue;
					}

					parameter.CachedTime = DateTime.Now;
					_settings.TryAdd(parameter.Name, parameter);
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
