using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Metrics;
using ZabbixSender.Async;
using ZabbixAsyncSender = ZabbixSender.Async.Sender;

namespace Vodovoz.Zabbix.Sender
{
	public partial class VodovozZabbixSender : IZabbixSender
	{
		private string _workerName;
		private readonly IMetricSettings _metricSettings;
		private readonly ILogger<VodovozZabbixSender> _logger;

		public VodovozZabbixSender(string workerName, IMetricSettings metricSettings, ILogger<VodovozZabbixSender> logger)
		{
			_workerName = workerName ?? throw new ArgumentNullException(nameof(workerName));
			_metricSettings = metricSettings ?? throw new ArgumentNullException(nameof(metricSettings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<bool> SendIsHealthyAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Отправляем информацию \"Работает\" в zabbix по {WorkerName}.", _workerName);

			if(!_metricSettings.ZabbixNeedSendMetrics)
			{
				_logger.LogInformation("Для текущей БД отключена отправка метрики в zabbix.");

				return false;
			}

			var sender = new ZabbixAsyncSender(_metricSettings.ZabbixUrl, timeout: 5000);

			SenderResponse response = null;

			try
			{
				response = await sender.Send(_metricSettings.ZabbixHost, _workerName, "Up", cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка отправки данных в zabbix.");

				return false;
			}

			return GetResponseResult(response);
		}

		public async Task<bool> SendIsUnhealthyAsync(ZabixSenderMessageType zabixSenderMessageType, string message, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Отправляем информацию о проблеме в zabbix по {WorkerName}.", _workerName);

			if(!_metricSettings.ZabbixNeedSendMetrics)
			{
				_logger.LogInformation("Для текущей БД отключена отправка метрики в zabbix.");

				return false;
			}

			var sender = new ZabbixAsyncSender(_metricSettings.ZabbixUrl, timeout: 5000);

			SenderResponse response = null;

			var senderValue = $"{zabixSenderMessageType}:{message}";

			try
			{
				response = await sender.Send(_metricSettings.ZabbixHost, _workerName, senderValue, cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка отправки данных в zabbix.");

				return false;
			}

			return GetResponseResult(response);
		}

		private bool GetResponseResult(SenderResponse response)
		{
			var responseInfo = response.Info
				.Replace("; ", ";")
				.Split(';')
				.Select(value => value.Split(':'))
				.ToDictionary(pair => pair[0], pair => pair[1]);

			int failedCount = 0;
			var failedInfoKey = "failed";

			var isResponseInfoParsed = responseInfo.ContainsKey(failedInfoKey)
				&& int.TryParse(responseInfo[failedInfoKey], out failedCount);

			if(!isResponseInfoParsed)
			{
				_logger.LogError("Неизвестный ответ zabbix или ошибка парсинга.");

				return false;
			}

			if(failedCount > 0)
			{
				_logger.LogError("Метрика отправлена, но не получена zabbix.");

				return false;
			}
			else
			{
				_logger.LogInformation("Метрика успешно отправлена в zabbix.");

				return true;
			}
		}
	}
}
