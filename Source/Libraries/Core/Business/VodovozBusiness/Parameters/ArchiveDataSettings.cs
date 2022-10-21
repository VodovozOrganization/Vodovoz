using System;

namespace Vodovoz.Parameters
{
	public class ArchiveDataSettings : IArchiveDataSettings
	{
		private readonly IParametersProvider _parametersProvider;

		public ArchiveDataSettings(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int GetMonitoringPeriodAvailableInDays => _parametersProvider.GetIntValue("monitoring_period_available_in_days");
		public int GetDistanceCacheDataPeriodAvailable => _parametersProvider.GetIntValue("distance_cache_data_period_available");
		public string GetDatabaseNameForOldMonitoringAvailable => _parametersProvider.GetStringValue("database_for_old_monitoring_available");
	}
}
