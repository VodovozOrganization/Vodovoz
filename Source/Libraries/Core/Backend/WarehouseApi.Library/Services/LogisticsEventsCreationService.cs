using LogisticsEventsApi.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Warehouse;

namespace WarehouseApi.Library.Services
{
	public class LogisticsEventsCreationService : IDisposable, ILogisticsEventsCreationService
	{
		private const string _eventCreationEndpointAddress = "api/CompleteDriverWarehouseEventWithoutCoordinates";

		private readonly ILogger<LogisticsEventsCreationService> _logger;
		private readonly ILogisticsEventsSettings _logisticsEventsSettings;
		private static HttpClient _httpClient;

		public LogisticsEventsCreationService(
			ILogger<LogisticsEventsCreationService> logger,
			ILogisticsEventsSettings logisticsEventsSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_logisticsEventsSettings = logisticsEventsSettings ?? throw new ArgumentNullException(nameof(logisticsEventsSettings));

			_httpClient = new HttpClient { BaseAddress = new Uri(_logisticsEventsSettings.BaseUrl) };
		}

		public async Task<bool> CreateStartLoadingWarehouseEvent(int documentId, string accessToken, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Добавляем событие начала погрузки талона #{DocumentId}", documentId);

			var eventData = new DriverWarehouseEventQrData
			{
				DocumentId = documentId,
				EventId = _logisticsEventsSettings.CarLoadDocumentStartLoadEventId
			};

			SetHttpHeaders(accessToken);
			var httpContent = CreateHttpContent(eventData);

			var response = await _httpClient.PostAsync(_eventCreationEndpointAddress, httpContent, cancellationToken);
			var responseBody = await response.Content.ReadAsStreamAsync();

			_logger.LogInformation(GetResponseInfoMessage(response, eventData));

			return response.IsSuccessStatusCode;
		}

		public async Task<bool> CreateEndLoadingWarehouseEvent(int documentId, string accessToken, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Добавляем событие окончания погрузки талона #{DocumentId}", documentId);

			var eventData = new DriverWarehouseEventQrData
			{
				DocumentId = documentId,
				EventId = _logisticsEventsSettings.CarLoadDocumentEndLoadEventId
			};

			SetHttpHeaders(accessToken);
			var httpContent = CreateHttpContent(eventData);

			var response = await _httpClient.PostAsync(_eventCreationEndpointAddress, httpContent, cancellationToken);
			var responseBody = await response.Content.ReadAsStreamAsync();

			_logger.LogInformation(GetResponseInfoMessage(response, eventData));

			return response.IsSuccessStatusCode;
		}

		private StringContent CreateHttpContent(DriverWarehouseEventQrData eventData)
		{
			var content = JsonSerializer.Serialize(eventData);
			var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
			return httpContent;
		}

		private void SetHttpHeaders(string accessToken)
		{
			_httpClient.DefaultRequestHeaders.Accept.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			if(!string.IsNullOrEmpty(accessToken))
			{
				_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			}
		}

		private string GetResponseInfoMessage(HttpResponseMessage response, DriverWarehouseEventQrData eventData) =>
			$"Запрос добавления события вернул ответ со статусом {response.StatusCode}. " +
			$"Параметры запроса:" +
			$"\n\tBaseAddress: {_httpClient.BaseAddress.AbsoluteUri}, " +
			$"\n\tEndpointAddress: {_eventCreationEndpointAddress}, " +
			$"\n\tToken: {_httpClient.DefaultRequestHeaders.Authorization.Parameter}, " +
			$"\n\tDocumentId: {eventData.DocumentId}, " +
			$"\n\tEventId: {eventData.EventId} ";

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}
