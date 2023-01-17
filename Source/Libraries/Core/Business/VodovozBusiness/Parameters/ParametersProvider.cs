using NHibernate.Persister.Entity;
using NLog;
using QS.DomainModel.UoW;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain;

namespace Vodovoz.Parameters
{
	[Obsolete("Вместо него необходимо использовать интерфейс Vodovoz.Settings.ISettingsController")]
	public class ParametersProvider : IParametersProvider
	{
		private readonly Logger logger = LogManager.GetCurrentClassLogger();

		private static readonly ConcurrentDictionary<string, BaseParameter> _parameters = new ConcurrentDictionary<string, BaseParameter>();

		public bool ContainsParameter(string parameterName)
		{
			if(_parameters.ContainsKey(parameterName))
			{
				return true;
			}
			
			RefreshParameter(parameterName);
			return _parameters.ContainsKey(parameterName);
		}

		public string GetParameterValue(string parameterName)
		{
			if(string.IsNullOrWhiteSpace(parameterName))
			{
				throw new ArgumentNullException(nameof(parameterName));
			}

			if(!ContainsParameter(parameterName))
			{
				throw new InvalidProgramException($"В параметрах базы не найден параметр ({parameterName})");
			}

			var parameter = _parameters[parameterName];
			if(parameter.IsExpired)
			{
				RefreshParameter(parameterName);
				parameter = _parameters[parameterName];
			}
			
			if(String.IsNullOrWhiteSpace(parameter.StrValue))
			{
				throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterName})");
			}
			
			return parameter.StrValue;
		}

		private void RefreshParameter(string parameterName)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				BaseParameter parameter = uow.Session.QueryOver<BaseParameter>()
					.Where(x => x.Name == parameterName)
					.SingleOrDefault<BaseParameter>();
				
				if(parameter == null)
				{
					return;
				}

				parameter.CachedTime = DateTime.Now;
				
				if(_parameters.ContainsKey(parameterName))
				{
					_parameters[parameterName] = parameter;
					return;
				}
				
				_parameters.TryAdd(parameter.Name, parameter);
			}
		}

		public void RefreshParameters()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var allParameters = uow.Session.QueryOver<BaseParameter>().List();
				_parameters.Clear();
				
				foreach(var parameter in allParameters)
				{
					if(_parameters.ContainsKey(parameter.Name))
					{
						continue;
					}
					
					parameter.CachedTime = DateTime.Now;
					_parameters.TryAdd(parameter.Name, parameter);
				}
			}
		}

		public int GetIntValue(string parameterId)
		{
			if(!ContainsParameter(parameterId))
			{
				throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})");
			}

			string value = GetParameterValue(parameterId);

			if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
			{
				throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
			}

			return result;
		}

		public char GetCharValue(string parameterId)
		{
			if(!ContainsParameter(parameterId))
			{
				throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})");
			}

			string value = GetParameterValue(parameterId);

			if(string.IsNullOrWhiteSpace(value) || !char.TryParse(value, out char result))
			{
				throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
			}

			return result;
		}

		public decimal GetDecimalValue(string parameterId)
		{
			if(!ContainsParameter(parameterId))
			{
				throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})");
			}

			string value = GetParameterValue(parameterId);

			if(string.IsNullOrWhiteSpace(value) || !decimal.TryParse(value, out decimal result))
			{
				throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
			}

			return result;
		}

		public string GetStringValue(string parameterId)
		{
			if(!ContainsParameter(parameterId))
			{
				throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})");
			}

			string value = GetParameterValue(parameterId);

			if(string.IsNullOrWhiteSpace(value))
			{
				throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
			}

			return value;
		}

		public T GetValue<T>(string parameterId)
		{
			if(!ContainsParameter(parameterId))
			{
				throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})");
			}

			string value = GetParameterValue(parameterId);
			if(string.IsNullOrWhiteSpace(value))
			{
				throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
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
				throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})", e);
			}

			return result;
		}

		public void CreateOrUpdateParameter(string name, string value)
		{
			bool isInsert = false;
			if(_parameters.TryGetValue(name, out var oldParameter))
			{
				if(oldParameter.StrValue == value)
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
				var bpPersister = (AbstractEntityPersister)uow.Session.SessionFactory.GetClassMetadata(typeof(BaseParameter));
				var tableName = bpPersister.TableName;
				var nameColumnName = bpPersister.GetPropertyColumnNames(nameof(BaseParameter.Name)).First();
				var strValueColumnName = bpPersister.GetPropertyColumnNames(nameof(BaseParameter.StrValue)).First();

				string sql;
				if(isInsert)
				{
					sql = $"INSERT INTO {tableName} ({nameColumnName}, {strValueColumnName}) VALUES ('{name}', '{value}')";
					logger.Debug($"Добавляем новый параметр базы {name}='{value}'");
				}
				else
				{
					sql = $"UPDATE {tableName} SET {strValueColumnName} = '{value}' WHERE {nameColumnName} = '{name}'";
					logger.Debug($"Изменяем параметр базы {name}='{value}'");
				}
				uow.Session.CreateSQLQuery(sql).ExecuteUpdate();
				RefreshParameter(name);
			}

			logger.Debug("Ок");
		}

		public bool GetBoolValue(string parameterId)
		{
			if(!ContainsParameter(parameterId))
			{
				throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})");
			}

			string value = GetParameterValue(parameterId);

			if(string.IsNullOrWhiteSpace(value) || !bool.TryParse(value, out bool result))
			{
				throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
			}

			return result;
		}
	}
}
