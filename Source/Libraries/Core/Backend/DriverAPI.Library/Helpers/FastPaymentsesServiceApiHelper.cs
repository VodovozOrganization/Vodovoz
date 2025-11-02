using System;
using System.Net.Http;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Responses;
using Microsoft.Extensions.Configuration;

namespace DriverAPI.Library.Helpers
{
	public class FastPaymentsesServiceApiHelper : IFastPaymentsServiceAPIHelper
	{
		private string _sendPaymentEndpointURI = "SendPayment";
		private readonly HttpClient _apiClient;

		public FastPaymentsesServiceApiHelper(
			IConfiguration configuration,
			HttpClient httpClient)
		{
			_apiClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			InitializeClient(configuration);
		}

		public async Task<QRResponseDTO> SendPaymentAsync(int orderId)
		{
			using(HttpResponseMessage response = await _apiClient.PostAsJsonAsync(_sendPaymentEndpointURI, new { OrderId = orderId }))
			{
				if(response.IsSuccessStatusCode)
				{
					return await response.Content.ReadAsAsync<QRResponseDTO>();
				}
				
				throw new SmsPaymentServiceAPIHelperException(response.ReasonPhrase);
			}
		}

		private void InitializeClient(IConfiguration configuration)
		{
			var apiConfiguration = configuration.GetSection("FastPaymentsServiceAPI");
			_sendPaymentEndpointURI = apiConfiguration.GetValue<string>("RegisterOrderForGetQREndpointURI");
		}
	}
}
