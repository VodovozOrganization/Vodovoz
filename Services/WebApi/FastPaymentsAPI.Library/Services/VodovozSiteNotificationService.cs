using FastPaymentsAPI.Library.DTO_s.Requests;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FastPaymentsAPI.Library.Services
{
	public class VodovozSiteNotificationService : IVodovozSiteNotificationService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private JsonSerializerOptions _jsonOptions;

		public VodovozSiteNotificationService(HttpClient client, IConfiguration configuration)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			ConfigureJsonOptions();
		}

		public async Task NotifyOfFastPaymentStatusChangedAsync(FastPaymentStatusChangeNotificationDto paymentNotificationDto)
		{
			var json = JsonSerializer.Serialize(paymentNotificationDto, _jsonOptions);
			var response = await _httpClient.PostAsJsonAsync(
				_configuration.GetSection("VodovozSiteNotificationService")
					.GetValue<string>("NotifyOfFastPaymentStatusChangedURI"), json);

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
