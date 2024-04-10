using Mango.Core.Settings;
using Microsoft.Extensions.Logging;
using System;

namespace Mango.Api.Validators
{
	public class KeyValidator
	{
		private readonly ILogger<KeyValidator> _logger;
		private readonly IMangoConfigurationSettings _mangoSettings;

		public KeyValidator(ILogger<KeyValidator> logger, IMangoConfigurationSettings mangoSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
		}

		public bool Validate(string vpbxKey)
		{
			var result = _mangoSettings.VpbxApiKey == vpbxKey;
			if(!result)
			{
				_logger.LogError("Vpbx ключ в запросе ({requestKey}) не совпадает с ключем в настройках ({settingKey}).",
					vpbxKey, _mangoSettings.VpbxApiKey);
			}

			return result;
		}
	}
}
