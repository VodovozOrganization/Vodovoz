using System;
using Vodovoz.Settings.Mango;

namespace Vodovoz.Settings.Database.Mango
{
	public class MangoUserSettings : IMangoUserSettngs
	{
		private readonly ISettingsController _settingsController;

		public MangoUserSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string TestCallsGroup => _settingsController.GetStringValue("Mango.TestCallsGroup");
	}
}
