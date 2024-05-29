using System;
using System.Threading.Tasks;
using Vodovoz.Settings.Metrics;
using ZabbixAsyncSender = ZabbixSender.Async.Sender;

namespace Vodovoz.Zabbix.Sender
{
	public class VodovozZabbixSender : IZabbixSender
	{
		private readonly string _workerName;
		private readonly IMetricSettings _metricSettings;

		public VodovozZabbixSender(string workerName, IMetricSettings metricSettings)
		{
			_workerName = workerName ?? throw new ArgumentNullException(nameof(workerName));
			_metricSettings = metricSettings?? throw new ArgumentNullException(nameof(metricSettings));
		}

		public async Task<bool> SendIsHealthyAsync(bool isHealthy)
		{
			var sender = new ZabbixAsyncSender(_metricSettings.ZabbixServer /*"192.168.133.129"*/);
			var response = await sender.Send("Vod Northlake" /*= это ZabbixHost*/, _workerName,  isHealthy.ToString());
			return response.IsSuccess;			
		}
	}
}
