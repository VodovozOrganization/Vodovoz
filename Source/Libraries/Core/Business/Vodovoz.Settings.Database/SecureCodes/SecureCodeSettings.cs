using System;
using Vodovoz.Settings.SecureCodes;

namespace Vodovoz.Settings.Database.SecureCodes
{
	public class SecureCodeSettings : ISecureCodeSettings
	{
		private readonly ISettingsController _settingsController;

		public SecureCodeSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int TimeForNextCodeSeconds => _settingsController.GetIntValue("SecureCodeSender.TimeForNextCode");
		public int CodeLifetimeSeconds => _settingsController.GetIntValue("SecureCodeSender.CodeLifetimeSeconds");
	}
}
