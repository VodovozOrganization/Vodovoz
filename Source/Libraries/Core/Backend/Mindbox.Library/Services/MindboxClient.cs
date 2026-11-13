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
using Mindbox.Library.Contracts;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
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
			if(httpClientFactory is null)
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
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			string externalId = null;

			try
			{
				externalId = request.Customer?.Ids?.ExternalId;
				var url = BuildOperationUrl(transactionId, request.DeviceUuid);
				var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

				using(var content = new StringContent(json, Encoding.UTF8, "application/json"))
				using(var httpRequest = CreateRequest(url, content))
				{
					_logger.LogDebug(
						"Отправка клиента в Mindbox: externalId={ExternalId}, transactionId={TransactionId}, operation={Operation}, pointOfContact={PointOfContact}, deviceUUID={DeviceUuid}",
						externalId,
						transactionId,
						_settings.CreateCustomerOperation,
						request.PointOfContact,
						request.DeviceUuid);

					var stopwatch = Stopwatch.StartNew();

					try
					{
						using(var response = await _httpClient.SendAsync(httpRequest, cancellationToken))
						{
							var rawResponse = await response.Content.ReadAsStringAsync();
							stopwatch.Stop();

							var result = MapResponse((int)response.StatusCode, rawResponse, stopwatch.ElapsedMilliseconds);

							LogResult(externalId, transactionId, result);

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

						LogResult(externalId, transactionId, result);

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

						LogResult(externalId, transactionId, result);

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
					"Ошибка подготовки запроса в Mindbox: externalId={ExternalId}, transactionId={TransactionId}",
					externalId,
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

		/// <inheritdoc />
		public async Task<MindboxOperationResult> CalculateProductListAsync(
			MindboxCalculateProductListRequest request,
			CancellationToken cancellationToken)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var stopwatch = Stopwatch.StartNew();

			try
			{
				var url = BuildCalculateProductListUrl();
				var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

				using(var content = new StringContent(json, Encoding.UTF8, "application/json"))
				using(var httpRequest = CreateRequest(url, content))
				{
					try
					{
						using(var response = await _httpClient.SendAsync(httpRequest, cancellationToken))
						{
							var rawResponse = await response.Content.ReadAsStringAsync();
							stopwatch.Stop();

							var result = MapResponse((int)response.StatusCode, rawResponse, stopwatch.ElapsedMilliseconds);

							LogResult(null, Guid.Empty, result, _settings.CalculateProductListOperation);

							return result;
						}
					}
					catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
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

						LogResult(null, Guid.Empty, result, _settings.CalculateProductListOperation);

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

						LogResult(null, Guid.Empty, result, _settings.CalculateProductListOperation);

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
					"Ошибка подготовки запроса CalculateProductList в Mindbox");

				return MindboxOperationResult.Failure(
					MindboxSendOutcome.ProtocolError,
					0,
					"RequestError",
					ex.Message,
					null,
					stopwatch.ElapsedMilliseconds,
					null);
			}
		}

		private string BuildCalculateProductListUrl()
		{
			var baseUrl = _settings.BaseUrl.TrimEnd('/');

			return $"{baseUrl}/v3/operations/sync"
				+ $"?endpointId={Uri.EscapeDataString(_settings.EndpointId)}"
				+ $"&operation={Uri.EscapeDataString(_settings.CalculateProductListOperation)}";
		}

		/// <inheritdoc />
		public async Task<MindboxOperationResult> CalculateAuthorizedCartAsync(
			MindboxAuthorizedCartRequest request,
			CancellationToken cancellationToken)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var stopwatch = Stopwatch.StartNew();
			var timeoutSeconds = _settings.AuthorizedCartTimeoutSeconds > 0
				? _settings.AuthorizedCartTimeoutSeconds
				: 3;

			try
			{
				var url = BuildAuthorizedCartUrl();
				var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

				using(var content = new StringContent(json, Encoding.UTF8, "application/json"))
				using(var httpRequest = CreateRequest(url, content))
				using(var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
				{
					timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

					try
					{
						using(var response = await _httpClient.SendAsync(httpRequest, timeoutCts.Token))
						{
							var rawResponse = await response.Content.ReadAsStringAsync();
							stopwatch.Stop();

							var result = MapResponse((int)response.StatusCode, rawResponse, stopwatch.ElapsedMilliseconds);

							LogResult(null, Guid.Empty, result, _settings.CalculateAuthorizedCartOperation);

							return result;
						}
					}
					catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
					{
						throw;
					}
					catch(OperationCanceledException)
					{
						stopwatch.Stop();

						var result = MindboxOperationResult.Transient(
							0,
							"Timeout",
							"Превышено время ожидания ответа Mindbox",
							stopwatch.ElapsedMilliseconds);

						LogResult(null, Guid.Empty, result, _settings.CalculateAuthorizedCartOperation);

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

						LogResult(null, Guid.Empty, result, _settings.CalculateAuthorizedCartOperation);

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
					"Ошибка подготовки запроса CalculateAuthorizedCart в Mindbox");

				return MindboxOperationResult.Failure(
					MindboxSendOutcome.ProtocolError,
					0,
					"RequestError",
					ex.Message,
					null,
					stopwatch.ElapsedMilliseconds,
					null);
			}
		}

		/// <inheritdoc />
		public async Task<MindboxOperationResult> CreateAuthorizedLoyaltyOrderAsync(
			MindboxLoyaltyOrderRequest request,
			Guid transactionId,
			CancellationToken cancellationToken)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var stopwatch = Stopwatch.StartNew();
			var timeoutSeconds = _settings.AuthorizedLoyaltyOrderTimeoutSeconds > 0
				? _settings.AuthorizedLoyaltyOrderTimeoutSeconds
				: 10;

			try
			{
				var url = BuildLoyaltyOrderUrl(transactionId);
				var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

				using(var content = new StringContent(json, Encoding.UTF8, "application/json"))
				using(var httpRequest = CreateRequest(url, content))
				using(var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
				{
					timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

					try
					{
						using(var response = await _httpClient.SendAsync(httpRequest, timeoutCts.Token))
						{
							var rawResponse = await response.Content.ReadAsStringAsync();
							stopwatch.Stop();

							var result = MapResponse((int)response.StatusCode, rawResponse, stopwatch.ElapsedMilliseconds);

							LogResult(null, transactionId, result, _settings.CreateAuthorizedLoyaltyOrderOperation);

							return result;
						}
					}
					catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
					{
						throw;
					}
					catch(OperationCanceledException)
					{
						stopwatch.Stop();

						var result = MindboxOperationResult.Transient(
							0,
							"Timeout",
							"Превышено время ожидания ответа Mindbox",
							stopwatch.ElapsedMilliseconds);

						LogResult(null, transactionId, result, _settings.CreateAuthorizedLoyaltyOrderOperation);

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

						LogResult(null, transactionId, result, _settings.CreateAuthorizedLoyaltyOrderOperation);

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
					"Ошибка подготовки запроса CreateAuthorizedLoyaltyOrder в Mindbox");

				return MindboxOperationResult.Failure(
					MindboxSendOutcome.ProtocolError,
					0,
					"RequestError",
					ex.Message,
					null,
					stopwatch.ElapsedMilliseconds,
					null);
			}
		}

		/// <inheritdoc />
		public async Task<MindboxOperationResult> UpdateOrderLinesStatusAsync(
			MindboxOrderLinesStatusRequest request,
			Guid transactionId,
			CancellationToken cancellationToken)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var stopwatch = Stopwatch.StartNew();

			try
			{
				var url = BuildOrderLinesStatusUrl(transactionId);
				var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

				using(var content = new StringContent(json, Encoding.UTF8, "application/json"))
				using(var httpRequest = CreateRequest(url, content))
				using(var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
				{
					timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

					try
					{
						using(var response = await _httpClient.SendAsync(httpRequest, timeoutCts.Token))
						{
							var rawResponse = await response.Content.ReadAsStringAsync();
							stopwatch.Stop();

							var result = MapResponse((int)response.StatusCode, rawResponse, stopwatch.ElapsedMilliseconds);

							LogResult(null, transactionId, result, _settings.UpdateOrderLinesStatusOperation);

							return result;
						}
					}
					catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
					{
						throw;
					}
					catch(OperationCanceledException)
					{
						stopwatch.Stop();

						return MindboxOperationResult.Transient(
							0,
							"Timeout",
							"Превышено время ожидания ответа Mindbox",
							stopwatch.ElapsedMilliseconds);
					}
					catch(HttpRequestException ex)
					{
						stopwatch.Stop();

						return MindboxOperationResult.Transient(
							0,
							"ConnectionError",
							ex.Message,
							stopwatch.ElapsedMilliseconds);
					}
				}
			}
			catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка подготовки запроса изменения статуса позиций Mindbox");

				return MindboxOperationResult.Failure(
					MindboxSendOutcome.ProtocolError,
					0,
					"RequestError",
					ex.Message,
					null,
					stopwatch.ElapsedMilliseconds,
					null);
			}
		}

		/// <inheritdoc />
		public async Task<MindboxOperationResult> GetOrderInfoAsync(
			MindboxGetOrderInfoRequest request,
			CancellationToken cancellationToken)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var stopwatch = Stopwatch.StartNew();

			try
			{
				var url = BuildGetOrderInfoUrl();
				var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

				using(var content = new StringContent(json, Encoding.UTF8, "application/json"))
				using(var httpRequest = CreateRequest(url, content))
				using(var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
				{
					timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

					try
					{
						using(var response = await _httpClient.SendAsync(httpRequest, timeoutCts.Token))
						{
							var rawResponse = await response.Content.ReadAsStringAsync();
							stopwatch.Stop();

							var result = MapResponse((int)response.StatusCode, rawResponse, stopwatch.ElapsedMilliseconds);

							LogResult(null, Guid.Empty, result, _settings.GetOrderInfoOperation);

							return result;
						}
					}
					catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
					{
						throw;
					}
					catch(OperationCanceledException)
					{
						stopwatch.Stop();

						return MindboxOperationResult.Transient(
							0,
							"Timeout",
							"Превышено время ожидания ответа Mindbox",
							stopwatch.ElapsedMilliseconds);
					}
					catch(HttpRequestException ex)
					{
						stopwatch.Stop();

						return MindboxOperationResult.Transient(
							0,
							"ConnectionError",
							ex.Message,
							stopwatch.ElapsedMilliseconds);
					}
				}
			}
			catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка подготовки запроса GetOrderInfo Mindbox");

				return MindboxOperationResult.Failure(
					MindboxSendOutcome.ProtocolError,
					0,
					"RequestError",
					ex.Message,
					null,
					stopwatch.ElapsedMilliseconds,
					null);
			}
		}

		/// <inheritdoc />
		public async Task<MindboxOperationResult> GetBalanceHistoryAsync(
			MindboxGetBalanceHistoryRequest request,
			CancellationToken cancellationToken)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var stopwatch = Stopwatch.StartNew();

			try
			{
				var url = BuildGetBalanceHistoryUrl();
				var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

				using(var content = new StringContent(json, Encoding.UTF8, "application/json"))
				using(var httpRequest = CreateRequest(url, content))
				using(var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
				{
					timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

					try
					{
						using(var response = await _httpClient.SendAsync(httpRequest, timeoutCts.Token))
						{
							var rawResponse = await response.Content.ReadAsStringAsync();
							stopwatch.Stop();

							var result = MapResponse((int)response.StatusCode, rawResponse, stopwatch.ElapsedMilliseconds);

							LogResult(null, Guid.Empty, result, _settings.GetBalanceHistoryOperation);

							return result;
						}
					}
					catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
					{
						throw;
					}
					catch(OperationCanceledException)
					{
						stopwatch.Stop();

						return MindboxOperationResult.Transient(
							0,
							"Timeout",
							"Превышено время ожидания ответа Mindbox",
							stopwatch.ElapsedMilliseconds);
					}
					catch(HttpRequestException ex)
					{
						stopwatch.Stop();

						return MindboxOperationResult.Transient(
							0,
							"ConnectionError",
							ex.Message,
							stopwatch.ElapsedMilliseconds);
					}
				}
			}
			catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка подготовки запроса GetBalanceHistory Mindbox");

				return MindboxOperationResult.Failure(
					MindboxSendOutcome.ProtocolError,
					0,
					"RequestError",
					ex.Message,
					null,
					stopwatch.ElapsedMilliseconds,
					null);
			}
		}

		private string BuildGetBalanceHistoryUrl()
		{
			var baseUrl = _settings.BaseUrl.TrimEnd('/');

			return $"{baseUrl}/v3/operations/sync"
				+ $"?endpointId={Uri.EscapeDataString(_settings.EndpointId)}"
				+ $"&operation={Uri.EscapeDataString(_settings.GetBalanceHistoryOperation)}";
		}

		private string BuildGetOrderInfoUrl()
		{
			var baseUrl = _settings.BaseUrl.TrimEnd('/');

			return $"{baseUrl}/v3/operations/sync"
				+ $"?endpointId={Uri.EscapeDataString(_settings.EndpointId)}"
				+ $"&operation={Uri.EscapeDataString(_settings.GetOrderInfoOperation)}";
		}

		private string BuildOrderLinesStatusUrl(Guid transactionId)
		{
			var baseUrl = _settings.BaseUrl.TrimEnd('/');

			return $"{baseUrl}/v3/operations/sync"
				+ $"?endpointId={Uri.EscapeDataString(_settings.EndpointId)}"
				+ $"&operation={Uri.EscapeDataString(_settings.UpdateOrderLinesStatusOperation)}"
				+ $"&transactionId={transactionId:D}";
		}

		private string BuildLoyaltyOrderUrl(Guid transactionId)
		{
			var baseUrl = _settings.BaseUrl.TrimEnd('/');

			return $"{baseUrl}/v3/operations/sync"
				+ $"?endpointId={Uri.EscapeDataString(_settings.EndpointId)}"
				+ $"&operation={Uri.EscapeDataString(_settings.CreateAuthorizedLoyaltyOrderOperation)}"
				+ $"&transactionId={transactionId:D}";
		}

		private string BuildAuthorizedCartUrl()
		{
			var baseUrl = _settings.BaseUrl.TrimEnd('/');

			return $"{baseUrl}/v3/operations/sync"
				+ $"?endpointId={Uri.EscapeDataString(_settings.EndpointId)}"
				+ $"&operation={Uri.EscapeDataString(_settings.CalculateAuthorizedCartOperation)}";
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

		private string BuildOperationUrl(Guid transactionId, string deviceUuid)
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

			var url = $"{baseUrl}/v3/operations/{operationType}"
				+ $"?endpointId={Uri.EscapeDataString(_settings.EndpointId)}"
				+ $"&operation={Uri.EscapeDataString(_settings.CreateCustomerOperation)}";

			if(!string.IsNullOrWhiteSpace(deviceUuid))
			{
				url += $"&deviceUUID={Uri.EscapeDataString(deviceUuid)}";
			}

			return url + $"&transactionId={transactionId:D}";
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

		private void LogResult(
			string externalId,
			Guid transactionId,
			MindboxOperationResult result,
			string operation = null)
		{
			_logger.LogDebug(
				"Ответ Mindbox: externalId={ExternalId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, outcome={Outcome}",
				externalId,
				transactionId,
				operation ?? _settings.CreateCustomerOperation,
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
			if(response is null)
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
