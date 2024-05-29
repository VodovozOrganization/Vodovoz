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

		public string ZabbixServer => _settingsController.GetStringValue(nameof(ZabbixServer));

		//public string ZabbixHealthMetricName => _settingsController.GetStringValue(nameof(ZabbixHealthMetricName));
	}
}
