using System.Collections.Generic;
using Vodovoz.Models.CashReceipts.DTO;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System;

namespace Vodovoz.Models.CashReceipts
{
	public class CashboxSettingProvider : ICashboxSettingProvider
	{
		private const string _configValueNotFoundString = "Не удалось прочитать значение параметра \"{0}\" из конфигурации настроек кассовых аппаратов.";
		private readonly IConfigurationSection _cashboxesConfiguration;
		private List<CashboxSetting> _settings = new List<CashboxSetting>();

		public CashboxSettingProvider(IConfigurationSection cashboxesConfiguration)
		{
			_cashboxesConfiguration = cashboxesConfiguration ?? throw new System.ArgumentNullException(nameof(cashboxesConfiguration));
			Setup();
		}

		private void Setup()
		{
			if(!_cashboxesConfiguration.Exists())
			{
				throw new ConfigurationErrorsException("Невозможно прочитать секцию конфигурации с настройками кассовых аппаратов.");
			}


			foreach(var cashboxConfig in _cashboxesConfiguration.GetChildren())
			{
				string stringId = cashboxConfig["id"];
				if(string.IsNullOrWhiteSpace(stringId) || !int.TryParse(stringId, out int id))
				{
					throw new ConfigurationErrorsException(string.Format(_configValueNotFoundString, "id"));
				}

				string retailPointName = cashboxConfig["retailPointName"];
				if(string.IsNullOrWhiteSpace(retailPointName))
				{
					throw new ConfigurationErrorsException(string.Format(_configValueNotFoundString, "retailPointName"));
				}

				string stringUserId = cashboxConfig["userId"];
				if(string.IsNullOrWhiteSpace(stringUserId) || !Guid.TryParse(stringUserId, out Guid userId))
				{
					throw new ConfigurationErrorsException(string.Format(_configValueNotFoundString, "userId"));
				}

				string password = cashboxConfig["password"];
				if(string.IsNullOrWhiteSpace(retailPointName))
				{
					throw new ConfigurationErrorsException(string.Format(_configValueNotFoundString, "password"));
				}

				var cashBox = new CashboxSetting
				{
					Id = id,
					RetailPointName = retailPointName,
					UserId = userId,
					Password = password
				};

				_settings.Add(cashBox);
			}
		}

		public IEnumerable<CashboxSetting> GetCashBoxSettings()
		{
			return _settings;
		}
	}
}
