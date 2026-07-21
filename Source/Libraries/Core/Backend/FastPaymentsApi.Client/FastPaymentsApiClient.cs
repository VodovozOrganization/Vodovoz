using FastPaymentsApi.Contracts.Requests;
using FastPaymentsApi.Contracts.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FastPaymentsApi.Client
{

	public class FastPaymentsApiClient : IFastPaymentsApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<FastPaymentsApiClient> _logger;
		private readonly JsonSerializerOptions _jsonOptions;

		public FastPaymentsApiClient(
			HttpClient httpClient,
			ILogger<FastPaymentsApiClient> logger,
			JsonSerializerOptions jsonOptions)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
		}

		public async Task<ReverseTicketResponseDTO> ReverseOrderAsync(
			ReverseTicketRequestDTO request,
			CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("Выполнение возврата заказа");

				var endpoint = "ReverseOrder";
				using var response = await _httpClient.PostAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);

				var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
				_logger.LogInformation("Ответ от API: {StatusCode}", response.StatusCode);

				if(!response.IsSuccessStatusCode)
				{
					_logger.LogWarning("Ошибка при возврате заказа: HTTP {StatusCode}",
						response.StatusCode);

					return new ReverseTicketResponseDTO($"HTTP {response.StatusCode}");
				}

				var result = JsonSerializer.Deserialize<ReverseTicketResponseDTO>(responseJson, _jsonOptions);
				return result;
			}
			catch(HttpRequestException ex)
			{
				_logger.LogError(ex, "Ошибка HTTP при возврате заказа");
				throw new Exception($"Ошибка соединения с FastPayment: {ex.Message}", ex);
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "Ошибка десериализации ответа");
				throw new Exception($"Ошибка формата ответа: {ex.Message}", ex);
			}
		}
	}
}
