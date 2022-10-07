using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FastPaymentsAPI.Library.Services
{
	public class DriverAPIService : IDriverAPIService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public DriverAPIService(HttpClient client, IConfiguration configuration)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public async Task NotifyOfFastPaymentStatusChangedAsync(int orderId)
		{
			var response = await _httpClient.PostAsJsonAsync(
				_configuration.GetSection("DriverAPIService").GetValue<string>("NotifyOfFastPaymentStatusChangedURI"), orderId);

			if(response.IsSuccessStatusCode)
			{
				return;
			}
			throw new Exception(response.ReasonPhrase);
		}
	}
}
