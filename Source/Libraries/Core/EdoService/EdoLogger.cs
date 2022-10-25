using System;
using NLog;
using System.Net.Http;
using System.Text;

namespace EdoService
{
	public class EdoLogger : IEdoLogger
	{
		private readonly ILogger _logger;

		public EdoLogger(ILogger logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		public void LogError(HttpResponseMessage response)
		{
			var statusCode = response.StatusCode;
			var reason = response.ReasonPhrase;
			_logger.Error($"Http code {statusCode}, причина {reason}");
		}
	}
}
