using System;
using System.Threading.Tasks;
using Vodovoz.Settings.Metrics;
using ZabbixAsyncSender = ZabbixSender.Async.Sender;

namespace Vodovoz.Zabbix.Sender
{
	public class VodovozZabbixSender : IZabbixSender
	{
		private readonly ZabbixAsyncSender _sender;
		private readonly string _workerName;
		private readonly IMetricSettings _metricSettings;

		public VodovozZabbixSender(string workerName, IMetricSettings metricSettings)
		{
			_workerName = workerName ?? throw new ArgumentNullException(nameof(workerName));
			_metricSettings = metricSettings?? throw new ArgumentNullException(nameof(metricSettings));
			_sender = new ZabbixAsyncSender(metricSettings.ZabbixHost);
		}

		public async Task SendIsHealthyAsync(bool isHealthy)
		{
			var response = await _sender.Send(_workerName, _metricSettings.ZabbixHealthMetricName, isHealthy.ToString());
		}
	}
}
