using FastPaymentsApi.Contracts.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FastPaymentsAPI.Library.Services
{
	public class OrderService : IOrderService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly ILogger<OrderService> _logger;

		public OrderService(HttpClient client, IConfiguration configuration, ILogger<OrderService> logger)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<OrderRegistrationResponseDTO> RegisterOrderAsync(string xmlStringOrderRegistrationRequestDTO)
		{
			const int maxAttempts = 3;

			var endpoint = _configuration.GetSection("OrderService").GetValue<string>("RegisterOrderEndpointURI");

			Exception lastException = null;

			for(var attempt = 1; attempt <= maxAttempts; attempt++)
			{
				try
				{
					using var httpContent = new StringContent($"xml={xmlStringOrderRegistrationRequestDTO}", Encoding.UTF8, "application/x-www-form-urlencoded");
					using var response = await _httpClient.PostAsync(endpoint, httpContent);

					if(response.IsSuccessStatusCode)
					{
						using var stream = await response.Content.ReadAsStreamAsync();
						return (OrderRegistrationResponseDTO)new XmlSerializer(typeof(OrderRegistrationResponseDTO)).Deserialize(stream);
					}

					var responseBody = await response.Content.ReadAsStringAsync();

					var truncatedBody = responseBody.Substring(0, Math.Min(300, responseBody.Length));

					_logger.LogWarning(
						"OrderService.RegisterOrder attempt {Attempt}/{MaxAttempts} failed. Status={StatusCode}, Body={Body}",
						attempt, maxAttempts, (int)response.StatusCode, truncatedBody);

					lastException = new InvalidOperationException($"OrderService вернул {response.StatusCode}. Body: {truncatedBody}");
				}
				catch(HttpRequestException ex) when(attempt < maxAttempts)
				{
					lastException = ex;
					_logger.LogWarning(ex, "OrderService.RegisterOrder network error on attempt {Attempt}/{MaxAttempts}", attempt, maxAttempts);
				}
				catch(TaskCanceledException ex) when(attempt < maxAttempts)
				{
					lastException = ex;
					_logger.LogWarning(ex, "OrderService.RegisterOrder timeout on attempt {Attempt}/{MaxAttempts}", attempt, maxAttempts);
				}

				if(attempt < maxAttempts)
				{
					await Task.Delay(400 * attempt);
				}
			}

			_logger.LogError(lastException, "OrderService.RegisterOrder failed after {MaxAttempts} attempts", maxAttempts);

			throw lastException ?? new InvalidOperationException("Не удалось выполнить RegisterOrder");
		}

		public async Task<OrderInfoResponseDTO> GetOrderInfoAsync(string xmlStringOrderInfoDTO)
        {
            var httpContent = new StringContent($"xml={xmlStringOrderInfoDTO}", Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await _httpClient.PostAsync(
            _configuration.GetSection("OrderService").GetValue<string>("GetOrderInfoEndpointURI"), httpContent);

            using var responseStream = await response.Content.ReadAsStreamAsync();
            return (OrderInfoResponseDTO)new XmlSerializer(typeof(OrderInfoResponseDTO)).Deserialize(responseStream);
        }
		
		public async Task<CancelPaymentResponseDTO> CancelPaymentAsync(string xmlStringFromCancelPaymentRequestDTO)
		{
			var endpoint = _configuration.GetSection("OrderService").GetValue<string>("CancelPaymentEndpointURI");
			var httpContent = new StringContent(
				$"xml={xmlStringFromCancelPaymentRequestDTO}",
				Encoding.UTF8,
				"application/x-www-form-urlencoded");

			var response = await _httpClient.PostAsync(endpoint, httpContent);

			using var responseStream = await response.Content.ReadAsStreamAsync();
			return (CancelPaymentResponseDTO)new XmlSerializer(typeof(CancelPaymentResponseDTO)).Deserialize(responseStream);
		}

		public async Task<ReverseOrderResponseDTO> ReverseOrderAsync(string xmlStringReverseOrderRequestDTO)
		{
			var endpoint = _configuration.GetSection("OrderService").GetValue<string>("ReverseOrderEndpointURI");
			var httpContent = new StringContent(
				$"xml={xmlStringReverseOrderRequestDTO}",
				Encoding.UTF8,
				"application/x-www-form-urlencoded");

			var response = await _httpClient.PostAsync(endpoint, httpContent);

			using var responseStream = await response.Content.ReadAsStreamAsync();
			return (ReverseOrderResponseDTO)new XmlSerializer(typeof(ReverseOrderResponseDTO)).Deserialize(responseStream);
		}
	}
}
