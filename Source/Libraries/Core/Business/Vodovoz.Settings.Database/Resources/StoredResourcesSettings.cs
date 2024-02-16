using System;
using Vodovoz.Settings.Resources;

namespace Vodovoz.Settings.Database.Resources
{
	public class StoredResourcesSettings : IStoredResourcesSettings
	{
		private readonly ISettingsController _settingsController;

		public StoredResourcesSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int GetIconFolderStoredResourceId => _settingsController.GetValue<int>("icon_folder_stored_resource_id");
	}
}
