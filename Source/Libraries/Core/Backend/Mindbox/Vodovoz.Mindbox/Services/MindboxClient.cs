using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vodovoz.Mindbox.Contracts;
using Vodovoz.Settings.Mindbox;

namespace Vodovoz.Mindbox.Services
{
	/// <inheritdoc />
	public sealed class MindboxClient : IMindboxClient
	{
		private const string HttpClientName = nameof(MindboxClient);
		private const string SuccessStatus = "Success";
		private const string TransactionAlreadyProcessedStatus = "TransactionAlreadyProcessed";

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
			_httpClient.Timeout = TimeSpan.FromSeconds(120);
		}

		/// <inheritdoc />
		public async Task<MindboxOperationResult> CreateCounterpartyAsync(
			MindboxCounterpartyRequest request,
			Guid transactionId,
			CancellationToken cancellationToken)
		{
			if(request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var url = BuildOperationUrl(_settings.CreateCounterpartyOperation, transactionId);
			var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

			using(var content = new StringContent(json, Encoding.UTF8, "application/json"))
			using(var httpRequest = CreateRequest(url, content))
			{
				_logger.LogInformation(
					"Отправка контрагента {CounterpartyId} в Mindbox, операция {Operation}, transactionId {TransactionId}",
					request.ErpCounterpartyId,
					_settings.CreateCounterpartyOperation,
					transactionId);

				return await SendAsync(httpRequest, cancellationToken);
			}
		}

		/// <inheritdoc />
		public async Task<MindboxOperationResult> ImportCustomersAsync(
			byte[] csvContent,
			Guid transactionId,
			CancellationToken cancellationToken)
		{
			if(csvContent == null)
			{
				throw new ArgumentNullException(nameof(csvContent));
			}

			var url = BuildBulkImportUrl(transactionId);

			using(var content = new ByteArrayContent(csvContent))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("text/csv")
				{
					CharSet = "utf-8"
				};

				using(var httpRequest = CreateRequest(url, content))
				{
					_logger.LogInformation(
						"Отправка CSV в Mindbox, {Bytes} байт, transactionId {TransactionId}",
						csvContent.Length,
						transactionId);

					return await SendAsync(httpRequest, cancellationToken);
				}
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

		private async Task<MindboxOperationResult> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			try
			{
				using(var response = await _httpClient.SendAsync(request, cancellationToken))
				{
					var rawResponse = await response.Content.ReadAsStringAsync();

					if(!response.IsSuccessStatusCode)
					{
						var errorMessage = $"Mindbox вернул HTTP {(int)response.StatusCode} {response.StatusCode}";
						_logger.LogWarning("{ErrorMessage}. Ответ: {Response}", errorMessage, rawResponse);

						return MindboxOperationResult.Failure((int)response.StatusCode, errorMessage, rawResponse);
					}

					var mindboxResponse = TryDeserialize(rawResponse);

					if(mindboxResponse == null)
					{
						return MindboxOperationResult.Success((int)response.StatusCode, null, rawResponse);
					}

					if(IsSuccessStatus(mindboxResponse.Status))
					{
						return MindboxOperationResult.Success(
							(int)response.StatusCode,
							mindboxResponse.Status,
							rawResponse);
					}

					var validationError = BuildValidationError(mindboxResponse);
					_logger.LogWarning("Mindbox вернул ошибку: {Error}. Ответ: {Response}", validationError, rawResponse);

					return MindboxOperationResult.Failure((int)response.StatusCode, validationError, rawResponse);
				}
			}
			catch(TaskCanceledException ex)
			{
				_logger.LogError(ex, "Превышено время ожидания ответа Mindbox");
				return MindboxOperationResult.Failure(0, "Превышено время ожидания ответа Mindbox");
			}
			catch(HttpRequestException ex)
			{
				_logger.LogError(ex, "Ошибка HTTP-запроса в Mindbox");
				return MindboxOperationResult.Failure(0, ex.Message);
			}
		}

		private string BuildOperationUrl(string operation, Guid transactionId)
		{
			var baseUrl = _settings.BaseUrl.TrimEnd('/');
			var operationType = string.IsNullOrWhiteSpace(_settings.CreateCounterpartyOperationType)
				? "async"
				: _settings.CreateCounterpartyOperationType.Trim().ToLowerInvariant();

			if(operationType != "sync" && operationType != "async")
			{
				throw new InvalidOperationException(
					$"Некорректный тип операции Mindbox: {_settings.CreateCounterpartyOperationType}. Ожидается sync или async.");
			}

			return $"{baseUrl}/v3/operations/{operationType}"
				+ $"?endpointId={Uri.EscapeDataString(_settings.EndpointId)}"
				+ $"&operation={Uri.EscapeDataString(operation)}"
				+ $"&transactionId={transactionId:D}";
		}

		private string BuildBulkImportUrl(Guid transactionId)
		{
			var baseUrl = _settings.BaseUrl.TrimEnd('/');
			var query = new List<string>
			{
				$"endpointId={Uri.EscapeDataString(_settings.EndpointId)}",
				$"operation={Uri.EscapeDataString(_settings.ImportOperation)}",
				"csvCodePage=65001",
				"csvColumnDelimiter=%3B",
				"csvTextQualifier=%22",
				$"transactionId={transactionId:D}"
			};

			if(!string.IsNullOrWhiteSpace(_settings.ImportSegment))
			{
				query.Add($"segment={Uri.EscapeDataString(_settings.ImportSegment)}");
			}

			if(!string.IsNullOrWhiteSpace(_settings.ImportSourceActionTemplate))
			{
				query.Add($"SourceActionTemplate={Uri.EscapeDataString(_settings.ImportSourceActionTemplate)}");
			}

			return $"{baseUrl}/v3/operations/bulk?{string.Join("&", query)}";
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

		private static bool IsSuccessStatus(string status)
		{
			if(string.IsNullOrWhiteSpace(status))
			{
				return true;
			}

			return status.Equals(SuccessStatus, StringComparison.OrdinalIgnoreCase)
				|| status.Equals(TransactionAlreadyProcessedStatus, StringComparison.OrdinalIgnoreCase);
		}

		private static string BuildValidationError(MindboxResponse response)
		{
			if(!string.IsNullOrWhiteSpace(response.ErrorMessage))
			{
				return response.ErrorMessage;
			}

			if(response.ValidationMessages == null || response.ValidationMessages.Count == 0)
			{
				return $"Mindbox вернул статус {response.Status}";
			}

			var messages = new List<string>();

			foreach(var validationMessage in response.ValidationMessages)
			{
				messages.Add(string.IsNullOrWhiteSpace(validationMessage.Location)
					? validationMessage.Message
					: $"{validationMessage.Location}: {validationMessage.Message}");
			}

			return string.Join("; ", messages);
		}
	}
}
