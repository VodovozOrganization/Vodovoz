using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Requests;

namespace FastPaymentsAPI.Library.ApiClients
{
	public class MobileAppClient : IDisposable
	{
		private readonly ILogger<MobileAppClient> _logger;
		private readonly HttpClient _httpClient;

		public MobileAppClient(ILogger<MobileAppClient> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
		}

		public async Task<bool> NotifyPaymentStatusChangedAsync(FastPaymentStatusChangeNotificationDto paymentNotificationDto, string url)
		{
			if(string.IsNullOrWhiteSpace(url))
			{
				throw new ArgumentException($"'{nameof(url)}' cannot be null or whitespace.", nameof(url));
			}

			var content = JsonSerializer.Serialize(paymentNotificationDto);
			var response = await _httpClient.PostAsJsonAsync(url, content);

			if(response.IsSuccessStatusCode)
			{
				return true;
			}

			_logger.LogInformation(response.ReasonPhrase);
			return false;
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}
