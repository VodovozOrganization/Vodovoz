using System;
using Vodovoz.Settings.Metrics;

namespace Vodovoz.Settings.Database.Metric
{
	public class MetricSettings : IMetricSettings
	{
		private readonly ISettingsController _settingsController;

		public MetricSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string ZabbixHost => _settingsController.GetStringValue(nameof(ZabbixHost));
		public string ZabbixUrl => _settingsController.GetStringValue(nameof(ZabbixUrl));
		public bool ZabbixNeedSendMetrics => _settingsController.GetBoolValue(nameof(ZabbixNeedSendMetrics));
	}
}
