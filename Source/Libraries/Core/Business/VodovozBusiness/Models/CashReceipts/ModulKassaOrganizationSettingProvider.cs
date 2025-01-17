using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using VodovozBusiness.Models.CashReceipts.DTO;

namespace Vodovoz.Models.CashReceipts
{
	public class ModulKassaOrganizationSettingProvider : IModulKassaOrganizationSettingProvider
	{
		private const string _configValueNotFoundString = "Не удалось прочитать значение параметра \"{0}\" из конфигурации настроек организаций.";
		private readonly IConfigurationSection _modulKassaOrganizationsConfiguration;
		private List<ModulKassaOrganizationSetting> _settings = new List<ModulKassaOrganizationSetting>();

		public ModulKassaOrganizationSettingProvider(IConfigurationSection modulKassaOrganizationesConfiguration)
		{
			_modulKassaOrganizationsConfiguration = modulKassaOrganizationesConfiguration ?? throw new ArgumentNullException(nameof(modulKassaOrganizationesConfiguration));
			Setup();
		}

		private void Setup()
		{
			if(!_modulKassaOrganizationsConfiguration.Exists())
			{
				throw new ConfigurationErrorsException("Невозможно прочитать секцию конфигурации с настройками организаций.");
			}


			foreach(var modulKassaOrganizationConfig in _modulKassaOrganizationsConfiguration.GetChildren())
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

				var modulKassaOrganization = new ModulKassaOrganizationSetting
				{
					OrganizationId = organizationId,
					HeaderApiKey = headerApiKey,
				};

				_settings.Add(modulKassaOrganization);
			}
		}

		public IEnumerable<ModulKassaOrganizationSetting> GetModulKassaOrganizationSettings()
		{
			return _settings;
		}
	}
}
