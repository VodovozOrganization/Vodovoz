﻿using Microsoft.Extensions.Configuration;
using System;

namespace Mango.Core.Settings
{
	public class ConfigurationMangoSettings : IMangoSettings
	{
		private const string _sectionName = "Mango";
		private const string _keySettingName = "VpbxApiKey";
		private const string _saltSettingName = "VpbxApiSalt";

		private string _vpbxApiKey;
		private string _vpbxApiSalt;

		public ConfigurationMangoSettings(IConfiguration configuration)
		{
			var mangoSection = configuration.GetSection(_sectionName);
			if(!mangoSection.Exists())
			{
				throw new ArgumentException($"Не найдена секция {_sectionName} в конфигурации");
			}

			_vpbxApiKey = mangoSection[_keySettingName];
			_vpbxApiSalt = mangoSection[_saltSettingName];

			if(string.IsNullOrWhiteSpace(_vpbxApiKey))
			{
				throw new ArgumentException($"Настройка {_keySettingName} не определена в конфигурации");
			}

			if(string.IsNullOrWhiteSpace(_vpbxApiSalt))
			{
				throw new ArgumentException($"Настройка {_saltSettingName} не определена в конфигурации");
			}
		}

		public string VpbxApiKey => _vpbxApiKey;

		public string VpbxApiSalt => _vpbxApiSalt;
	}
}
