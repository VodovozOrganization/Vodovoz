using System;
using System.Net.Http;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s.Requests;
using Microsoft.Extensions.Configuration;

namespace FastPaymentsAPI.Library.Services
{
	public class MobileAppNotificationService : IMobileAppNotificationService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public MobileAppNotificationService(HttpClient client, IConfiguration configuration)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public async Task NotifyOfFastPaymentStatusChangedAsync(FastPaymentStatusChangeNotificationDto paymentNotificationDto)
		{
			var response = await _httpClient.PostAsJsonAsync(
				_configuration.GetSection("MobileAppNotificationService")
					.GetValue<string>("NotifyOfFastPaymentStatusChangedURI"), paymentNotificationDto);

			if(response.IsSuccessStatusCode)
			{
				return;
			}
			throw new Exception(response.ReasonPhrase);
		}
	}
}
