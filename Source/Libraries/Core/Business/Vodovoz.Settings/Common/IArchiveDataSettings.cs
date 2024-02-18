namespace Vodovoz.Settings.Common
{
	public interface IArchiveDataSettings
	{
		int GetMonitoringPeriodAvailableInDays { get; }
		int GetDistanceCacheDataPeriodAvailable { get; }
		string GetDatabaseNameForOldMonitoringAvailable { get; }
	}
}
