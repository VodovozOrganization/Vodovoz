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

		public string ZabbixHealthMetricName => _settingsController.GetStringValue(nameof(ZabbixHealthMetricName));
	}
}
