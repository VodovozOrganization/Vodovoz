using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WarehouseApi.Library.Options;

namespace WarehouseApi.Library.Services
{
	public class LogisticsEventsCreationService : IDisposable
	{
		private const string _eventCreationEndpointAddress = "api/CompleteDriverWarehouseEvent";

		private readonly ILogger<LogisticsEventsCreationService> _logger;
		private readonly IOptions<LogisticsEventsApiSettings> _logisticsEventsApiSettings;
		private static HttpClient _httpClient;

		public LogisticsEventsCreationService(
			ILogger<LogisticsEventsCreationService> logger,
			IOptions<LogisticsEventsApiSettings> logisticsEventsApiSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_logisticsEventsApiSettings = logisticsEventsApiSettings ?? throw new ArgumentNullException(nameof(logisticsEventsApiSettings));

			_httpClient = new HttpClient { BaseAddress = new Uri(logisticsEventsApiSettings.Value.Address) };
			_httpClient.DefaultRequestHeaders.Accept.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", logisticsEventsApiSettings.Value.Token);
		}

		//public async Task<bool> CreateStartLoadingWarehouseEvent(int documentId, CancellationToken cancellationToken)
		//{

		//}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}
