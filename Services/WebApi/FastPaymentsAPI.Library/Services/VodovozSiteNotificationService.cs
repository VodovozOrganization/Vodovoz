using FastPaymentsAPI.Library.DTO_s.Requests;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FastPaymentsAPI.Library.Services
{
	public class VodovozSiteNotificationService : IVodovozSiteNotificationService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public VodovozSiteNotificationService(HttpClient client, IConfiguration configuration)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public async Task NotifyOfFastPaymentStatusChangedAsync(VodovozSiteNotificationPaymentRequestDto paymentNotificationDto)
		{
			var response = await _httpClient.PostAsJsonAsync(
				_configuration.GetSection("VodovozSiteNotificationService").GetValue<string>("NotifyOfFastPaymentStatusChangedURI"), paymentNotificationDto);

			if(response.IsSuccessStatusCode)
			{
				return;
			}
			throw new Exception(response.ReasonPhrase);
		}
	}
}
