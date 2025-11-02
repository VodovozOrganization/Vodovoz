using Mango.Core.Settings;
using System;

namespace Mango.Core.Sign
{
	public class DefaultSignGenerator : IDefaultSignGenerator
	{
		private readonly ISignGenerator _signGenerator;
		private readonly IMangoConfigurationSettings _mangoSettings;

		public DefaultSignGenerator(ISignGenerator signGenerator, IMangoConfigurationSettings mangoSettings)
		{
			_signGenerator = signGenerator ?? throw new ArgumentNullException(nameof(signGenerator));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
		}

		public string GetSign(string json)
		{
			var key = _mangoSettings.VpbxApiKey;
			var salt = _mangoSettings.VpbxApiSalt;

			return _signGenerator.GetSign(key, salt, json);
		}
	}
}
