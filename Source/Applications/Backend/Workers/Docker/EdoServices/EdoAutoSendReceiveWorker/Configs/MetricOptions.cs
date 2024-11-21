using Vodovoz.Settings.Metrics;

namespace EdoAutoSendReceiveWorker.Configs
{
	internal class MetricOptions : IMetricSettings
	{
		public const string Path = nameof(MetricOptions);

		public bool ZabbixNeedSendMetrics { get; set; }

		public string ZabbixHost { get; set; }

		public string ZabbixUrl { get; set; }
	}
}
