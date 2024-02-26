using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;

namespace Vodovoz.Settings.Database.Common
{
	public class GMapSettings : IGMapSettings
	{
		private readonly ISettingsController _settingsController;

		public GMapSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string SquidServer => _settingsController.GetStringValue("squidServer");
	}
}
