using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Mindbox.Contracts;
using Vodovoz.Settings.Database.Mindbox;

namespace Vodovoz.Mindbox.Services
{
	/// <inheritdoc />
	public sealed class MindboxClient : IMindboxClient
	{
		private const string HttpClientName = nameof(MindboxClient);
		private const string SuccessStatus = "Success";
		private const string TransactionAlreadyProcessedStatus = "TransactionAlreadyProcessed";
		private const string ValidationErrorStatus = "ValidationError";

		private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		private readonly HttpClient _httpClient;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxClient> _logger;

		public MindboxClient(
			IHttpClientFactory httpClientFactory,
			IMindboxSettings settings,
			ILogger<MindboxClient> logger)
		{
			if(httpClientFactory == null)
			{
				throw new ArgumentNullException(nameof(httpClientFactory));
			}

			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClient = httpClientFactory.CreateClient(HttpClientName);

			var timeoutSeconds = _settings.TimeoutSeconds > 0
				? _settings.TimeoutSeconds
				: 10;

			_httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
		}

		/// <inheritdoc />
		public async Task<MindboxOperationResult> CreateCustomerAsync(
			MindboxCustomerRequest request,
			Guid transactionId,
			CancellationToken cancellationToken)
		{
			if(request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			string erpId = null;

			try
			{
				erpId = request.Customer?.Ids?.ErpId;
				var url = BuildOperationUrl(transactionId);
				var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

				using(var content = new StringContent(json, Encoding.UTF8, "application/json"))
				using(var httpRequest = CreateRequest(url, content))
				{
					_logger.LogDebug(
						"Отправка клиента в Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}",
						erpId,
						transactionId,
						_settings.CreateCustomerOperation);

					var stopwatch = Stopwatch.StartNew();

					try
					{
						using(var response = await _httpClient.SendAsync(httpRequest, cancellationToken))
						{
							var rawResponse = await response.Content.ReadAsStringAsync();
							stopwatch.Stop();

							var result = MapResponse((int)response.StatusCode, rawResponse, stopwatch.ElapsedMilliseconds);

							LogResult(erpId, transactionId, result);

							return result;
						}
					}
					catch(TaskCanceledException) when(cancellationToken.IsCancellationRequested)
					{
						throw;
					}
					catch(TaskCanceledException)
					{
						stopwatch.Stop();

						var result = MindboxOperationResult.Transient(
							0,
							"Timeout",
							"Превышено время ожидания ответа Mindbox",
							stopwatch.ElapsedMilliseconds);

						LogResult(erpId, transactionId, result);

						return result;
					}
					catch(HttpRequestException ex)
					{
						stopwatch.Stop();

						var result = MindboxOperationResult.Transient(
							0,
							"ConnectionError",
							ex.Message,
							stopwatch.ElapsedMilliseconds);

						LogResult(erpId, transactionId, result);

						return result;
					}
				}
			}
			catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка подготовки запроса в Mindbox: erpId={ErpId}, transactionId={TransactionId}",
					erpId,
					transactionId);

				return MindboxOperationResult.Failure(
					MindboxSendOutcome.ProtocolError,
					0,
					"RequestError",
					ex.Message,
					null,
					0,
					null);
			}
		}

		private HttpRequestMessage CreateRequest(string url, HttpContent content)
		{
			var request = new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = content
			};

			request.Headers.Authorization = new AuthenticationHeaderValue("SecretKey", _settings.SecretKey);
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			return request;
		}

		private string BuildOperationUrl(Guid transactionId)
		{
			var baseUrl = _settings.BaseUrl.TrimEnd('/');
			var operationType = string.IsNullOrWhiteSpace(_settings.CreateCustomerOperationType)
				? "sync"
				: _settings.CreateCustomerOperationType.Trim().ToLowerInvariant();

			if(operationType != "sync" && operationType != "async")
			{
				throw new InvalidOperationException(
					$"Некорректный тип операции Mindbox: {_settings.CreateCustomerOperationType}. Ожидается sync или async.");
			}

			return $"{baseUrl}/v3/operations/{operationType}"
				+ $"?endpointId={Uri.EscapeDataString(_settings.EndpointId)}"
				+ $"&operation={Uri.EscapeDataString(_settings.CreateCustomerOperation)}"
				+ $"&transactionId={transactionId:D}";
		}

		private static MindboxOperationResult MapResponse(int httpStatusCode, string rawResponse, long durationMs)
		{
			var response = TryDeserialize(rawResponse);
			var status = response?.Status;
			var errorMessage = BuildErrorMessage(response, httpStatusCode);
			var errorId = response?.ErrorId;

			if(httpStatusCode == 200)
			{
				if(string.IsNullOrWhiteSpace(status)
					|| status.Equals(SuccessStatus, StringComparison.OrdinalIgnoreCase)
					|| status.Equals(TransactionAlreadyProcessedStatus, StringComparison.OrdinalIgnoreCase))
				{
					return MindboxOperationResult.Success(httpStatusCode, status, durationMs, rawResponse);
				}

				if(status.Equals(ValidationErrorStatus, StringComparison.OrdinalIgnoreCase))
				{
					return MindboxOperationResult.Failure(
						MindboxSendOutcome.ValidationError,
						httpStatusCode,
						status,
						errorMessage,
						errorId,
						durationMs,
						rawResponse);
				}

				return MindboxOperationResult.Failure(
					MindboxSendOutcome.ProtocolError,
					httpStatusCode,
					status,
					errorMessage,
					errorId,
					durationMs,
					rawResponse);
			}

			switch(httpStatusCode)
			{
				case 400:
					return MindboxOperationResult.Failure(
						MindboxSendOutcome.ProtocolError,
						httpStatusCode,
						status,
						errorMessage,
						errorId,
						durationMs,
						rawResponse);
				case 401:
				case 403:
					return MindboxOperationResult.Failure(
						MindboxSendOutcome.Unauthorized,
						httpStatusCode,
						status,
						errorMessage,
						errorId,
						durationMs,
						rawResponse);
				case 404:
					return MindboxOperationResult.Failure(
						MindboxSendOutcome.NotFound,
						httpStatusCode,
						status,
						errorMessage,
						errorId,
						durationMs,
						rawResponse);
				case 429:
					return MindboxOperationResult.Failure(
						MindboxSendOutcome.RateLimited,
						httpStatusCode,
						status,
						errorMessage,
						errorId,
						durationMs,
						rawResponse);
				case 500:
				case 502:
				case 503:
				case 504:
					return MindboxOperationResult.Failure(
						MindboxSendOutcome.TransientError,
						httpStatusCode,
						status,
						errorMessage,
						errorId,
						durationMs,
						rawResponse);
				default:
					// Остальные ответы (в том числе 501, 505) не повторяем:
					// повтор вернёт тот же результат.
					return MindboxOperationResult.Failure(
						MindboxSendOutcome.ProtocolError,
						httpStatusCode,
						status,
						errorMessage,
						errorId,
						durationMs,
						rawResponse);
			}
		}

		private void LogResult(string erpId, Guid transactionId, MindboxOperationResult result)
		{
			_logger.LogDebug(
				"Ответ Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, outcome={Outcome}",
				erpId,
				transactionId,
				_settings.CreateCustomerOperation,
				result.HttpStatusCode,
				result.Status,
				result.ErrorId,
				result.DurationMs,
				result.Outcome);
		}

		private static MindboxResponse TryDeserialize(string rawResponse)
		{
			if(string.IsNullOrWhiteSpace(rawResponse))
			{
				return null;
			}

			try
			{
				return JsonSerializer.Deserialize<MindboxResponse>(rawResponse);
			}
			catch(JsonException)
			{
				return null;
			}
		}

		private static string BuildErrorMessage(MindboxResponse response, int httpStatusCode)
		{
			if(response == null)
			{
				return $"Mindbox вернул HTTP {httpStatusCode}";
			}

			if(!string.IsNullOrWhiteSpace(response.ErrorMessage))
			{
				return response.ErrorMessage;
			}

			if(response.ValidationMessages != null && response.ValidationMessages.Count > 0)
			{
				var messages = new List<string>();

				foreach(var validationMessage in response.ValidationMessages)
				{
					messages.Add(string.IsNullOrWhiteSpace(validationMessage.Location)
						? validationMessage.Message
						: $"{validationMessage.Location}: {validationMessage.Message}");
				}

				return string.Join("; ", messages);
			}

			return string.IsNullOrWhiteSpace(response.Status)
				? $"Mindbox вернул HTTP {httpStatusCode}"
				: $"Mindbox вернул статус {response.Status}";
		}
	}
}
