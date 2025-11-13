using Microsoft.Extensions.Hosting;
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
		private readonly IHostEnvironment _hostEnvironment;

		public VodovozZabbixSender(string workerName, IMetricSettings metricSettings, ILogger<VodovozZabbixSender> logger, IHostEnvironment hostEnvironment)
		{
			_workerName = workerName ?? throw new ArgumentNullException(nameof(workerName));
			_metricSettings = metricSettings ?? throw new ArgumentNullException(nameof(metricSettings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
		}

		private bool CanSendMetrics()
		{
			if(_hostEnvironment.IsDevelopment())
			{
				_logger.LogInformation("В девелопе отключена отправка метрики", _workerName);

				return false;
			}

			if(!_metricSettings.ZabbixNeedSendMetrics)
			{
				_logger.LogInformation("В настройках отключена отправка метрики в zabbix.");

				return false;
			}

			return true;
		}

		public async Task<bool> SendIsHealthyAsync(CancellationToken cancellationToken)
		{
			if(!CanSendMetrics())
			{
				return false;
			}

			_logger.LogInformation("Отправляем информацию \"Работает\" в zabbix по {WorkerName}.", _workerName);

			var sender = new ZabbixAsyncSender(_metricSettings.ZabbixUrl, timeout: 5000);

			SenderResponse response = null;

			try
			{
				response = await sender.Send(_metricSettings.ZabbixHost, _workerName, ZabixSenderMessageType.Up.ToString(), cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка отправки данных в zabbix.");

				return false;
			}

			return GetResponseResult(response);
		}

		public async Task<bool> SendProblemMessageAsync(ZabixSenderMessageType zabixSenderMessageType, string message, CancellationToken cancellationToken)
		{
			if(!CanSendMetrics())
			{
				return false;
			}

			_logger.LogInformation("Отправляем сообщение в zabbix по {WorkerName}.", _workerName);

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
