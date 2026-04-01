using CloudPaymentsApi.Library.Models;
using CloudPaymentsApi.Library.Requests;
using CloudPaymentsApi.Library.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CloudPaymentsApi.Client
{
	/// <summary>
	/// API клиент для интеграции с CloudPayments
	/// </summary>
	public class CloudPaymentsApiClient : ICloudPaymentsApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<CloudPaymentsApiClient> _logger;
		private readonly JsonSerializerOptions _jsonOptions;

		public CloudPaymentsApiClient(
			HttpClient httpClient,
			ILogger<CloudPaymentsApiClient> logger,
			JsonSerializerOptions jsonOptions)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
		}

		public async Task<CloudPaymentsResponse<CloudPaymentsTransaction>> GetTransactionAsync(
			long transactionId,
			CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogDebug("Запрос информации о транзакции {TransactionId}", transactionId);

				var request = new { TransactionId = transactionId };
				var response = await PostAsync<CloudPaymentsTransaction>("payments/get", request, cancellationToken);

				if(response?.Success is true)
				{
					_logger.LogDebug("Транзакция {TransactionId} получена успешно, статус: {Status}",
						transactionId, response.Model?.Status);
				}
				else
				{
					_logger.LogWarning("Не удалось получить транзакцию {TransactionId}: {Message}",
						transactionId, response?.Message);
				}

				return response;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при получении транзакции {TransactionId}", transactionId);
				throw;
			}
		}

		public async Task<CloudPaymentsResponse<CloudPaymentsRefundResult>> RefundAsync(
			CloudPaymentsRefundRequest request,
			string idempotenceKey,
			CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("Выполнение возврата для транзакции {TransactionId}, сумма: {Amount}",
					request.TransactionId, request.Amount);

				var response = await PostAsync<CloudPaymentsRefundResult>("payments/refund", request, cancellationToken, idempotenceKey);

				if(response?.Success is true)
				{
					_logger.LogInformation("Возврат успешно создан, ID транзакции возврата: {RefundTransactionId}",
						response.Model?.TransactionId);
				}
				else
				{
					_logger.LogWarning("Не удалось выполнить возврат для {TransactionId}: {Message} (ErrorCode: {ErrorCode})",
						request.TransactionId, response?.Message, response?.ErrorCode);
				}

				return response;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при выполнении возврата для транзакции {TransactionId}",
					request.TransactionId);
			}
		}

		/// <summary>
		/// Общий метод для POST запросов к API CloudPayments
		/// </summary>
		private async Task<CloudPaymentsResponse<T>> PostAsync<T>(
			string endpoint,
			object data,
			CancellationToken cancellationToken,
			string idempotenceKey = null)
		{
			try
			{
				var json = JsonSerializer.Serialize(data, _jsonOptions);
				_logger.LogInformation("Отправка запроса на {Endpoint}: {Json}", endpoint, json);

				using var content = new StringContent(json, Encoding.UTF8, "application/json");

				var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
				{
					Content = content
				};

				if(!string.IsNullOrEmpty(idempotenceKey))
				{
					requestMessage.Headers.Add("X-Request-ID", idempotenceKey);
				}

				using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

				var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
				_logger.LogInformation("Ответ от {Endpoint}: {StatusCode} - {Response}",
					endpoint, response.StatusCode, responseJson);

				if(!response.IsSuccessStatusCode)
				{
					_logger.LogWarning("HTTP ошибка при вызове {Endpoint}: {StatusCode}",
						endpoint, response.StatusCode);

					var errorResponse = JsonSerializer.Deserialize<CloudPaymentsResponse<T>>(responseJson);
					if(errorResponse is not null)
					{
						return errorResponse;
					}

					return new CloudPaymentsResponse<T>
					{
						Success = false,
						Message = $"HTTP {response.StatusCode}: {responseJson}",
						ErrorCode = response.StatusCode.ToString()
					};
				}

				var result = JsonSerializer.Deserialize<CloudPaymentsResponse<T>>(responseJson, _jsonOptions);
				return result;
			}
			catch(HttpRequestException ex)
			{
				_logger.LogError(ex, "Ошибка HTTP запроса к {Endpoint}", endpoint);
				throw new Exception($"Ошибка соединения с CloudPayments: {ex.Message}", ex);
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "Ошибка десериализации ответа от {Endpoint}", endpoint);
				throw new Exception($"Ошибка формата ответа от CloudPayments: {ex.Message}", ex);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Неожиданная ошибка при запросе к {Endpoint}", endpoint);
				throw new Exception($"Неожиданная ошибка при обращении к CloudPayments: {ex.Message}", ex);
			}
		}
	}
}
