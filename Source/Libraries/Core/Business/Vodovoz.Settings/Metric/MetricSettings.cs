using System;

namespace Vodovoz.Settings.Metrics
{
	public class MetricSettings : IMetricSettings
	{
		public MetricSettings() { }

		public MetricSettings(ISettingsController settingsController)
		{
			settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
			ZabbixHost = settingsController.GetStringValue(nameof(ZabbixHost));
			ZabbixUrl = settingsController.GetStringValue(nameof(ZabbixUrl));
			ZabbixNeedSendMetrics = settingsController.GetBoolValue(nameof(ZabbixNeedSendMetrics));
		}

		public string ZabbixHost { get; set; }
		public string ZabbixUrl { get; set; }
		public bool ZabbixNeedSendMetrics { get; set; }
	}
}
