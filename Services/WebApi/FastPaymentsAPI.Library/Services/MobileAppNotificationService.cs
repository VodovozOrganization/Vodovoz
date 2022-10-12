using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s.Requests;

namespace FastPaymentsAPI.Library.Services
{
	public class MobileAppNotificationService : IMobileAppNotificationService
	{
		private readonly HttpClient _httpClient;
		private JsonSerializerOptions _jsonOptions;

		public MobileAppNotificationService(HttpClient client)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			ConfigureJsonOptions();
		}

		public async Task NotifyOfFastPaymentStatusChangedAsync(FastPaymentStatusChangeNotificationDto paymentNotificationDto, string url)
		{
			var json = JsonSerializer.Serialize(paymentNotificationDto, _jsonOptions);
			var response = await _httpClient.PostAsJsonAsync(url, json);

			if(response.IsSuccessStatusCode)
			{
				return;
			}
			throw new Exception(response.ReasonPhrase);
		}
		
		private void ConfigureJsonOptions()
		{
			_jsonOptions = new JsonSerializerOptions
			{
				WriteIndented = true
			};
		}
	}
}
