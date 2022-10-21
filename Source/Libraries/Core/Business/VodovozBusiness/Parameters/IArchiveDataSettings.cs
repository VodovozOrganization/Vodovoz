namespace Vodovoz.Parameters
{
	public interface IArchiveDataSettings
	{
		int GetMonitoringPeriodAvailableInDays { get; }
		int GetDistanceCacheDataPeriodAvailable { get; }
	}
}
