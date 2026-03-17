using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay;
using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.HttpClients
{
	public class YooKassaHttpClient : IYooKassaHttpClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<YooKassaHttpClient> _logger;
		private readonly JsonSerializerOptions _jsonOptions;

		public YooKassaHttpClient(
			HttpClient httpClient,
			ILogger<YooKassaHttpClient> logger,
			JsonSerializerOptions jsonOptions)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
		}

		public async Task<YooKassaResult<YooKassaPaymentResponse>> GetPaymentAsync(string paymentId, CancellationToken cancellationToken)
		{
			_logger.LogDebug("Запрос информации о платеже {PaymentId}", paymentId);

			var endpoint = $"payments/{paymentId}";
			var result = await GetAsync<YooKassaPaymentResponse>(endpoint, cancellationToken);

			if(result.Success)
			{
				_logger.LogDebug("Платеж {PaymentId} получен успешно, статус: {Status}",
					paymentId, result.Data?.Status);
			}
			else
			{
				_logger.LogWarning("Не удалось получить платеж {PaymentId}: {Message}",
					paymentId, result.ErrorMessage);
			}

			return result;
		}

		public async Task<YooKassaResult<YooKassaRefundResponse>> RefundAsync(
			YooKassaRefundRequest request,
			string idempotenceKey,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation("Выполнение возврата для платежа {PaymentId}, сумма: {Amount}, ключ идемпотентности: {IdempotenceKey}",
				request.PaymentId, request.Amount.Value, idempotenceKey);

			var endpoint = "refunds";
			var result = await PostAsync<YooKassaRefundResponse>(endpoint, request, idempotenceKey, cancellationToken);

			if(result.Success)
			{
				_logger.LogInformation("Возврат успешно создан, ID возврата: {RefundId}, статус: {Status}",
					result.Data?.Id, result.Data?.Status);
			}
			else
			{
				_logger.LogWarning("Не удалось выполнить возврат для {PaymentId}: {Message}",
					request.PaymentId, result.ErrorMessage);
			}

			return result;
		}

		/// <summary>
		/// Общий метод для GET запросов к API ЮKassa
		/// </summary>
		private async Task<YooKassaResult<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("GET запрос на {Endpoint}", endpoint);

				using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

				return await ProcessResponseAsync<T>(response, endpoint);
			}
			catch(HttpRequestException ex)
			{
				_logger.LogError(ex, "Ошибка HTTP запроса к {Endpoint}", endpoint);
				return YooKassaResult<T>.FromError($"Ошибка соединения: {ex.Message}");
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "Ошибка десериализации ответа от {Endpoint}", endpoint);
				return YooKassaResult<T>.FromError($"Ошибка формата ответа: {ex.Message}");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Непредвиденная ошибка при запросе к {Endpoint}", endpoint);
				return YooKassaResult<T>.FromError($"Внутренняя ошибка: {ex.Message}");
			}
		}

		/// <summary>
		/// Общий метод для POST запросов к API ЮKassa
		/// </summary>
		private async Task<YooKassaResult<T>> PostAsync<T>(
			string endpoint,
			object request,
			string idempotenceKey,
			CancellationToken cancellationToken)
		{
			try
			{
				var json = JsonSerializer.Serialize(request, _jsonOptions);
				_logger.LogInformation("POST запрос на {Endpoint}: {Json}", endpoint, json);

				using var content = new StringContent(json, Encoding.UTF8, "application/json");

				var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
				{
					Content = content
				};
				requestMessage.Headers.Add("Idempotence-Key", idempotenceKey);

				using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

				return await ProcessResponseAsync<T>(response, endpoint);
			}
			catch(HttpRequestException ex)
			{
				_logger.LogError(ex, "Ошибка HTTP запроса к {Endpoint}", endpoint);
				return YooKassaResult<T>.FromError($"Ошибка соединения: {ex.Message}");
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "Ошибка десериализации ответа от {Endpoint}", endpoint);
				return YooKassaResult<T>.FromError($"Ошибка формата ответа: {ex.Message}");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Непредвиденная ошибка при запросе к {Endpoint}", endpoint);
				return YooKassaResult<T>.FromError($"Внутренняя ошибка: {ex.Message}");
			}
		}

		/// <summary>
		/// Обработка ответа от API
		/// </summary>
		private async Task<YooKassaResult<T>> ProcessResponseAsync<T>(HttpResponseMessage response, string endpoint)
		{
			var responseJson = await response.Content.ReadAsStringAsync();

			_logger.LogInformation("Ответ от {Endpoint}: {StatusCode} - {Response}",
				endpoint, response.StatusCode, responseJson);

			if(!response.IsSuccessStatusCode)
			{
				return YooKassaResult<T>.FromError(
					$"HTTP ошибка {response.StatusCode}: {responseJson}",
					response.StatusCode.ToString());
			}

			try
			{
				var data = JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
				if(data != null)
				{
					return YooKassaResult<T>.FromSuccess(data);
				}
				return YooKassaResult<T>.FromError("Пустой ответ от API");
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "Ошибка десериализации ответа от {Endpoint}: {ResponseJson}",
					endpoint, responseJson);

				return YooKassaResult<T>.FromError(
					$"Некорректный формат ответа: {responseJson[..Math.Min(100, responseJson.Length)]}");
			}
		}
	}
}
