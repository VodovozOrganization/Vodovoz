using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace EdoService.Library
{
	public class EdoLogger : IEdoLogger
	{
		private readonly ILogger<EdoLogger> _logger;

		public EdoLogger(ILogger<EdoLogger> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public void LogError(HttpResponseMessage response)
		{
			var statusCode = response.StatusCode;
			var reason = response.ReasonPhrase;
			_logger.LogError("Http code {HttpCode}, причина {Reason}", statusCode, reason);
		}
	}
}
