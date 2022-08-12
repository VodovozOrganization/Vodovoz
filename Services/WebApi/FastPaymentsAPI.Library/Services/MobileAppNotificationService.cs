using System;
using System.Net.Http;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s.Requests;

namespace FastPaymentsAPI.Library.Services
{
	public class MobileAppNotificationService : IMobileAppNotificationService
	{
		private readonly HttpClient _httpClient;

		public MobileAppNotificationService(HttpClient client)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
		}

		public async Task NotifyOfFastPaymentStatusChangedAsync(FastPaymentStatusChangeNotificationDto paymentNotificationDto, string url)
		{
			var response = await _httpClient.PostAsJsonAsync(url, paymentNotificationDto);

			if(response.IsSuccessStatusCode)
			{
				return;
			}
			throw new Exception(response.ReasonPhrase);
		}
	}
}
