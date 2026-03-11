using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.HttpClients
{
	public class YandexPayHttpClient : IYandexPayHttpClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<YandexPayHttpClient> _logger;
		private readonly JsonSerializerOptions _jsonOptions;

		public YandexPayHttpClient(
			HttpClient httpClient,
			ILogger<YandexPayHttpClient> logger,
			JsonSerializerOptions jsonOptions)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
		}

		public async Task<YandexPayResult<YandexPayOrderResponse>> GetOrderAsync(string orderId)
		{
			_logger.LogDebug("Запрос информации о заказе {OrderId}", orderId);

			var endpoint = $"v1/orders/{orderId}";
			var result = await GetAsync<YandexPayOrderResponse>(endpoint);

			if(result.Success)
			{
				_logger.LogDebug("Заказ {OrderId} получен успешно, статус: {Status}",
					orderId, result.Data?.Order?.PaymentStatus);
			}

			return result;
		}

		public async Task<YandexPayResult<YandexPayRefundResponse>> RefundAsync(YandexPayRefundRequest request)
		{
			_logger.LogInformation("Выполнение возврата для заказа {OrderId}, сумма: {Amount}, externalOperationId: {ExternalId}",
				request.OrderId, request.RefundAmount, request.ExternalOperationId);

			var endpoint = $"v2/orders/{request.OrderId}/refund";
			var result = await PostAsync<YandexPayRefundResponse>(endpoint, request, request.CancellationToken);

			if(result.Success)
			{
				_logger.LogInformation("Возврат успешно создан, ID операции: {OperationId}, статус: {Status}",
					result.Data?.Operation?.OperationId, result.Data?.Operation?.Status);
			}
			else
			{
				_logger.LogWarning("Не удалось выполнить возврат для {OrderId}: {Message} (ReasonCode: {ReasonCode})",
					request.OrderId, result.ErrorMessage, result.ErrorCode);
			}

			return result;
		}

		/// <summary>
		/// Общий метод для GET запросов к API Яндекс Пэй
		/// </summary>
		private async Task<YandexPayResult<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
		{
			try
			{
				_logger.LogTrace("GET запрос на {Endpoint}", endpoint);

				using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

				return await ProcessResponseAsync<T>(response, endpoint);
			}
			catch(HttpRequestException ex)
			{
				_logger.LogError(ex, "Ошибка HTTP запроса к {Endpoint}", endpoint);
				return YandexPayResult<T>.FromError($"Ошибка соединения: {ex.Message}");
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "Ошибка десериализации ответа от {Endpoint}", endpoint);
				return YandexPayResult<T>.FromError($"Ошибка формата ответа: {ex.Message}");
			}
		}

		/// <summary>
		/// Общий метод для POST запросов к API Яндекс Пэй
		/// </summary>
		private async Task<YandexPayResult<T>> PostAsync<T>(string endpoint, YandexPayRefundRequest request, CancellationToken cancellationToken = default)
		{
			try
			{
				var apiRequest = new
				{
					refundAmount = request.RefundAmount,
					externalOperationId = request.ExternalOperationId,
					targetCart = request.TargetCart
				};

				var json = JsonSerializer.Serialize(apiRequest, _jsonOptions);
				_logger.LogTrace("POST запрос на {Endpoint}: {Json}", endpoint, json);

				using var content = new StringContent(json, Encoding.UTF8, "application/json");
				using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

				return await ProcessResponseAsync<T>(response, endpoint);
			}
			catch(HttpRequestException ex)
			{
				_logger.LogError(ex, "Ошибка HTTP запроса к {Endpoint}", endpoint);
				return YandexPayResult<T>.FromError($"Ошибка соединения: {ex.Message}");
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "Ошибка десериализации ответа от {Endpoint}", endpoint);
				return YandexPayResult<T>.FromError($"Ошибка формата ответа: {ex.Message}");
			}
		}

		/// <summary>
		/// Обработка ответа от API
		/// </summary>
		private async Task<YandexPayResult<T>> ProcessResponseAsync<T>(HttpResponseMessage response, string endpoint)
		{
			var responseJson = await response.Content.ReadAsStringAsync();

			_logger.LogTrace("Ответ от {Endpoint}: {StatusCode} - {Response}",
				endpoint, response.StatusCode, responseJson);

			if(!response.IsSuccessStatusCode)
			{
				return YandexPayResult<T>.FromError(
					$"HTTP ошибка {response.StatusCode}: {responseJson}",
					response.StatusCode.ToString());
			}

			try
			{
				var apiResponse = JsonSerializer.Deserialize<YandexPayApiResponse<T>>(responseJson, _jsonOptions);

				if(apiResponse?.Status == "success")
				{
					if(apiResponse.Data != null)
					{
						return YandexPayResult<T>.FromSuccess(apiResponse.Data);
					}
					return YandexPayResult<T>.FromError("Успешный ответ без данных");
				}

				return YandexPayResult<T>.FromError(
					apiResponse?.Reason ?? "Неизвестная ошибка",
					apiResponse?.ReasonCode,
					apiResponse?.Details);
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "Ошибка десериализации ответа от {Endpoint}: {ResponseJson}",
					endpoint, responseJson);

				return YandexPayResult<T>.FromError(
					$"Некорректный формат ответа: {responseJson[..Math.Min(100, responseJson.Length)]}");
			}
		}
	}
}
