using System;
using Vodovoz.Settings.Tabs;

namespace Vodovoz.Settings.Database.Tabs
{
	public class TabsSettings : ITabsSettings
	{
		private readonly ISettingsController _settingsController;

		public TabsSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public char TabsPrefix => _settingsController.GetCharValue("tab_prefix");
	}
}
