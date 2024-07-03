using System;
using System.Threading.Tasks;
using Vodovoz.Settings.Metrics;
using ZabbixAsyncSender = ZabbixSender.Async.Sender;

namespace Vodovoz.Zabbix.Sender
{
	public class VodovozZabbixSender : IZabbixSender
	{
		private string _workerName;
		private readonly IMetricSettings _metricSettings;

		public VodovozZabbixSender(string workerName, IMetricSettings metricSettings)
		{
			_workerName = workerName ?? throw new ArgumentNullException(nameof(workerName));
			_metricSettings = metricSettings?? throw new ArgumentNullException(nameof(metricSettings));
		}

		public void SetWorkerName(string workerName) => _workerName = workerName ?? throw new ArgumentNullException(nameof(workerName));

		public async Task<bool> SendIsHealthyAsync(bool isHealthy = true)
		{
			if(!_metricSettings.ZabbixNeedSendMetrics)
			{
				return false;
			}

			var healthy = isHealthy.ToString();

			var sender = new ZabbixAsyncSender(_metricSettings.ZabbixUrl);
			var response = await sender.Send(_metricSettings.ZabbixHost, _workerName, "Up");

			return response.IsSuccess;			
		}
	}
}
