using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vodovoz.Mindbox.Services;
using Vodovoz.Zabbix.Sender;

namespace MindboxCounterpartySender
{
	/// <summary>
	/// Отправляет алерты по ошибкам Mindbox в Zabbix и лог
	/// </summary>
	public sealed class ZabbixMindboxAlertSender : IMindboxAlertSender
	{
		private readonly IZabbixSender _zabbixSender;
		private readonly ILogger<ZabbixMindboxAlertSender> _logger;

		public ZabbixMindboxAlertSender(
			IZabbixSender zabbixSender,
			ILogger<ZabbixMindboxAlertSender> logger)
		{
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task AlertAsync(string message, CancellationToken cancellationToken)
		{
			_logger.LogError("Mindbox alert: {Message}", message);

			try
			{
				await _zabbixSender.SendProblemMessageAsync(
					nameof(MindboxCounterpartySender),
					ZabixSenderMessageType.Problem,
					message,
					cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Не удалось отправить алерт Mindbox в Zabbix");
			}
		}
	}
}
