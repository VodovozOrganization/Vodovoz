namespace Vodovoz.Settings.Metrics
{
	public interface IMetricSettings
	{
		bool ZabbixNeedSendMetrics { get; }
		string ZabbixHost { get; }
		string ZabbixUrl { get; }
	}
}
