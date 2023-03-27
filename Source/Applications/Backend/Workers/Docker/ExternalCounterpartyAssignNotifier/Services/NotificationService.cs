using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Vodovoz.Domain.Client;

namespace ExternalCounterpartyAssignNotifier.Services
{
	public class NotificationService : INotificationService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public NotificationService(HttpClient client, IConfiguration configuration)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public async Task<int> NotifyOfCounterpartyAssignAsync(
			RegisteredNaturalCounterpartyDto counterpartyDto, CounterpartyFrom counterpartyFrom)
		{
			var response = await _httpClient.PostAsJsonAsync(ConfigureHttpClient(counterpartyFrom), counterpartyDto);
			return (int)response.StatusCode;
		}

		private string ConfigureHttpClient(CounterpartyFrom counterpartyFrom)
		{
			switch(counterpartyFrom)
			{
				case CounterpartyFrom.MobileApp:
					_httpClient.BaseAddress =
						new Uri(_configuration.GetSection("MobileApp").GetValue<string>("BaseUrl"));
					return _configuration.GetSection("MobileApp").GetValue<string>("NotificationAddress");
				case CounterpartyFrom.WebSite:
					_httpClient.BaseAddress =
						new Uri(_configuration.GetSection("VodovozSite").GetValue<string>("BaseUrl"));
					return _configuration.GetSection("VodovozSite").GetValue<string>("NotificationAddress");
				default:
					throw new ArgumentOutOfRangeException(nameof(counterpartyFrom), counterpartyFrom, null);
			}
		}
	}
}
