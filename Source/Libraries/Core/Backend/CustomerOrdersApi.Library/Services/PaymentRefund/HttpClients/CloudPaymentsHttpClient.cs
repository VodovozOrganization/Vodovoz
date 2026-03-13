using CustomerOrdersApi.Library.Services.PaymentRefund.Models.CloudPayments;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using xNetStandard;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.HttpClients
{
	public class CloudPaymentsHttpClient : ICloudPaymentsHttpClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<CloudPaymentsHttpClient> _logger;
		private readonly JsonSerializerOptions _jsonOptions;

		public CloudPaymentsHttpClient(
			HttpClient httpClient,
			ILogger<CloudPaymentsHttpClient> logger,
			JsonSerializerOptions jsonOptions)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
		}

		public async Task<CloudPaymentsResponse<CloudPaymentsTransaction>> GetTransactionAsync(long transactionId, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogDebug("Запрос информации о транзакции {TransactionId}", transactionId);

				var request = new { TransactionId = transactionId };
				var response = await PostAsync<CloudPaymentsTransaction>("payments/get", request, cancellationToken);

				if(response?.Success == true)
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

		public async Task<CloudPaymentsResponse<CloudPaymentsRefundResult>> RefundAsync(CloudPaymentsRefundRequest request, CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("Выполнение возврата для транзакции {TransactionId}, сумма: {Amount}",
					request.TransactionId, request.Amount);

				var response = await PostAsync<CloudPaymentsRefundResult>("payments/refund", request, cancellationToken);

				if(response?.Success == true)
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
				throw;
			}
		}

		/// <summary>
		/// Общий метод для POST запросов к API CloudPayments
		/// </summary>
		private async Task<CloudPaymentsResponse<T>> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken)
		{
			try
			{
				var json = JsonSerializer.Serialize(data, _jsonOptions);
				_logger.LogTrace("Отправка запроса на {Endpoint}: {Json}", endpoint, json);

				using var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
				using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

				var responseJson = await response.Content.ReadAsStringAsync();
				_logger.LogTrace("Ответ от {Endpoint}: {StatusCode} - {Response}",
					endpoint, response.StatusCode, responseJson);

				if(!response.IsSuccessStatusCode)
				{
					_logger.LogWarning("HTTP ошибка при вызове {Endpoint}: {StatusCode}",
						endpoint, response.StatusCode);

					try
					{
						var errorResponse = JsonSerializer.Deserialize<CloudPaymentsResponse<T>>(responseJson);
						if(errorResponse != null)
						{
							return errorResponse;
						}
					}
					catch
					{
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
				throw new HttpException($"Ошибка соединения с CloudPayments: {ex.Message}", ex);
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "Ошибка десериализации ответа от {Endpoint}", endpoint);
				throw new HttpException($"Ошибка формата ответа от CloudPayments: {ex.Message}", ex);
			}
		}
	}
}
