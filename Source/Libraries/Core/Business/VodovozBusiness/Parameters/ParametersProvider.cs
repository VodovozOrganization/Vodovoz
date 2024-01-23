using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Project.Services;
using System;
using Vodovoz.Settings.Database;

namespace Vodovoz.Parameters
{
	[Obsolete("Вместо него необходимо использовать интерфейс Vodovoz.Settings.ISettingsController")]
	public class ParametersProvider : IParametersProvider
	{
		private readonly SettingsController _settingsController;

		public ParametersProvider()
		{
			var loggerFactory = new NLogLoggerFactory();
			_settingsController = new SettingsController(ServicesConfig.UnitOfWorkFactory, new Logger<SettingsController>(loggerFactory));
		}

		public bool ContainsParameter(string parameterName)
		{
			return _settingsController.ContainsSetting(parameterName);
		}

		public void CreateOrUpdateParameter(string name, string value)
		{
			_settingsController.CreateOrUpdateSetting(name, value);
		}

		public bool GetBoolValue(string parameterId)
		{
			return _settingsController.GetBoolValue(parameterId);
		}

		public char GetCharValue(string parameterId)
		{
			return _settingsController.GetCharValue(parameterId);
		}

		public decimal GetDecimalValue(string parameterId)
		{
			return _settingsController.GetDecimalValue(parameterId);
		}

		public int GetIntValue(string parameterId)
		{
			return _settingsController.GetIntValue(parameterId);
		}

		public string GetParameterValue(string parameterName, bool allowEmpty = false)
		{
			try
			{
				return _settingsController.GetStringValue(parameterName);
			}
			catch(Exception)
			{
				if(allowEmpty)
				{
					return string.Empty;
				}

				throw;
			}
		}

		public string GetStringValue(string parameterId)
		{
			return _settingsController.GetStringValue(parameterId);
		}

		public T GetValue<T>(string parameterId)
		{
			return _settingsController.GetValue<T>(parameterId);
		}

		public void RefreshParameters()
		{
			_settingsController.RefreshSettings();
		}
	}
}
