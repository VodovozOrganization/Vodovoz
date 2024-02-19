using NLog;
using System;
using System.Net.Http;

namespace EdoService.Library
{
	public class EdoLogger : IEdoLogger
	{
		private readonly ILogger _logger;

		public EdoLogger(ILogger logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public void LogError(HttpResponseMessage response)
		{
			var statusCode = response.StatusCode;
			var reason = response.ReasonPhrase;
			_logger.Error($"Http code {statusCode}, причина {reason}");
		}
	}
}
