using System;
using Vodovoz.Settings.Common;

namespace Vodovoz.Settings.Database.Common
{
	internal class WikiSettings : IWikiSettings
	{
		private readonly ISettingsController _settingsController;

		public WikiSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string Url => _settingsController.GetStringValue(nameof(WikiSettings) + "." + nameof(Url));
	}
}
