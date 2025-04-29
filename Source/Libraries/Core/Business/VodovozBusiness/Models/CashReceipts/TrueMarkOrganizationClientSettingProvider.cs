using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using VodovozBusiness.Models.CashReceipts.DTO;

namespace Vodovoz.Models.CashReceipts
{
	public class TrueMarkOrganizationClientSettingProvider : ITrueMarkOrganizationClientSettingProvider
	{
		private const string _configValueNotFoundString = "Не удалось прочитать значение параметра \"{0}\" из конфигурации настроек организаций.";
		private readonly IConfigurationSection _trueMarkOrganizationClientConfiguration;
		private List<TrueMarkOrganizationClientSetting> _settings = new List<TrueMarkOrganizationClientSetting>();

		public TrueMarkOrganizationClientSettingProvider(IConfigurationSection trueMarkOrganizationClientConfiguration)
		{
			_trueMarkOrganizationClientConfiguration = trueMarkOrganizationClientConfiguration ?? throw new ArgumentNullException(nameof(trueMarkOrganizationClientConfiguration));
			Setup();
		}

		private void Setup()
		{
			if(!_trueMarkOrganizationClientConfiguration.Exists())
			{
				throw new ConfigurationErrorsException("Невозможно прочитать секцию конфигурации с настройками организаций.");
			}
			
			foreach(var modulKassaOrganizationConfig in _trueMarkOrganizationClientConfiguration.GetChildren())
			{
				string stringOrganizationId = modulKassaOrganizationConfig["OrganizationId"];
				if(string.IsNullOrWhiteSpace(stringOrganizationId) || !int.TryParse(stringOrganizationId, out int organizationId))
				{
					throw new ConfigurationErrorsException(string.Format(_configValueNotFoundString, "OrganizationId"));
				}

				string stringHeaderApiKey = modulKassaOrganizationConfig["HeaderApiKey"];
				if(string.IsNullOrWhiteSpace(stringHeaderApiKey) || !Guid.TryParse(stringHeaderApiKey, out Guid headerApiKey))
				{
					throw new ConfigurationErrorsException(string.Format(_configValueNotFoundString, "HeaderApiKey"));
				}

				var modulKassaOrganization = new TrueMarkOrganizationClientSetting
				{
					OrganizationId = organizationId,
					HeaderTokenApiKey = headerApiKey,
				};

				_settings.Add(modulKassaOrganization);
			}
		}

		public IEnumerable<TrueMarkOrganizationClientSetting> GetModulKassaOrganizationSettings()
		{
			return _settings;
		}
	}
}
