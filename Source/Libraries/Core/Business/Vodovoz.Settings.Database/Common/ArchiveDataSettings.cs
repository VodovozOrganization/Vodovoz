using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Common;

namespace Vodovoz.Settings.Database.Common
{
	public class ArchiveDataSettings : IArchiveDataSettings
	{
		private readonly ISettingsController _settingsController;

		public ArchiveDataSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int GetMonitoringPeriodAvailableInDays => _settingsController.GetIntValue("monitoring_period_available_in_days");
		public int GetDistanceCacheDataPeriodAvailable => _settingsController.GetIntValue("distance_cache_data_period_available");
		public string GetDatabaseNameForOldMonitoringAvailable => _settingsController.GetStringValue("database_for_old_monitoring_available");
	}
}
