using Mango.Core.Settings;
using Mango.Api.Dto;
using Microsoft.Extensions.Logging;
using System;

namespace Mango.Api.Validators
{
	public class KeyValidator : IValidator
	{
		private readonly ILogger<KeyValidator> _logger;
		private readonly IMangoSettings _mangoSettings;

		public KeyValidator(ILogger<KeyValidator> logger, IMangoSettings mangoSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
		}

		public bool Validate(EventRequestBase eventRequest)
		{
			var result = _mangoSettings.VpbxApiKey == eventRequest.VpbxApiKey;
			if(!result)
			{
				_logger.LogError("Vpbx ключ в запросе ({requestKey}) не совпадает с ключем в настройках ({settingKey}).", 
					eventRequest.VpbxApiKey, _mangoSettings.VpbxApiKey);
			}

			return result;
		}
	}
}
