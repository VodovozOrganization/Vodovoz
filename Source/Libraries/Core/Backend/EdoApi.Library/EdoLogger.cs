using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace EdoApi.Library
{
	public class EdoLogger<T> : IEdoLogger
	{
		private readonly ILogger<T> _logger;

		public EdoLogger(ILogger<T> logger)
		{
			_logger = logger;
		}

		public void LogError(HttpResponseMessage response)
		{
			var statusCode = response.StatusCode;
			var reason = response.ReasonPhrase;
			_logger.LogError($"Http code {statusCode}, причина {reason}");
		}
	}
}
