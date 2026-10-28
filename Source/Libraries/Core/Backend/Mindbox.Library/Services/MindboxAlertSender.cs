using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Алерт только в лог; используется в консольных утилитах
	/// </summary>
	public sealed class LoggingMindboxAlertSender : IMindboxAlertSender
	{
		private readonly ILogger<LoggingMindboxAlertSender> _logger;

		public LoggingMindboxAlertSender(ILogger<LoggingMindboxAlertSender> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public Task AlertAsync(string message, CancellationToken cancellationToken)
		{
			_logger.LogError("Mindbox alert: {Message}", message);
			return Task.CompletedTask;
		}
	}
}
