using System;
using Vodovoz.Settings.Sale;

namespace Vodovoz.Settings.Database.Sale
{
	public class OnlineOrderTemplateSettings : IOnlineOrderTemplateSettings
	{
		private readonly ISettingsController _settingsController;

		public OnlineOrderTemplateSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}
		
		public int GetMaximumNumberActiveOnlineOrderTemplates =>
			_settingsController.GetValue<int>("OnlineOrderTemplate.MaximumNumberActiveOnlineOrderTemplates");
	}
}
