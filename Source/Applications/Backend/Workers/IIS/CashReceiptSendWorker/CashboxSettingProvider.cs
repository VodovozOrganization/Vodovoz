using System.Collections.Generic;
using Vodovoz.Models.CashReceipts.DTO;
using Vodovoz.Models.CashReceipts;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System;

namespace CashReceiptSendWorker
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

				string retailPointName = cashboxConfig["retail_point_name"];
				if(string.IsNullOrWhiteSpace(retailPointName))
				{
					throw new ConfigurationErrorsException(string.Format(_configValueNotFoundString, "retail_point_name"));
				}

				string stringUserId = cashboxConfig["user_id"];
				if(string.IsNullOrWhiteSpace(stringUserId) || !Guid.TryParse(stringUserId, out Guid userId))
				{
					throw new ConfigurationErrorsException(string.Format(_configValueNotFoundString, "user_id"));
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
