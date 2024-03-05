using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Logistics;

namespace Vodovoz.Settings.Database.Logistics
{
	public class GeographicGroupSettings : IGeographicGroupSettings
	{
		private readonly ISettingsController _settingsController;

		public GeographicGroupSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int SouthGeographicGroupId => _settingsController.GetIntValue("south_geographic_group_id");
		public int NorthGeographicGroupId => _settingsController.GetIntValue("north_geographic_group_id");
		public int EastGeographicGroupId => _settingsController.GetIntValue("east_geographic_group_id");
	}
}
