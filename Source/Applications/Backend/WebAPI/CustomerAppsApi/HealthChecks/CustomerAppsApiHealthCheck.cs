using CustomerAppsApi.Library.Dto;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace CustomerAppsApi.HealthChecks
{
	public class CustomerAppsApiHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public CustomerAppsApiHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			var httpClient = _httpClientFactory.CreateClient();
			httpClient.BaseAddress = new Uri($"{baseAddress}/api/");
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var request = new CounterpartyContactInfoDto
			{
				CameFromId = 54,
				ExternalCounterpartyId = new Guid("e48130ef-3cdf-4d65-85ef-f928c04e5aa4"),
				PhoneNumber = "9991327267"
			};

			var content = JsonSerializer.Serialize(request);
			var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

			var response = await httpClient.PostAsync("GetCounterparty", httpContent);
			var responseBody = await response.Content.ReadAsStreamAsync();
			var responseResult = await JsonSerializer.DeserializeAsync<CounterpartyIdentificationDto>(responseBody);

			var isHealthy = responseResult?.CounterpartyIdentificationStatus == CounterpartyIdentificationStatus.Success;

			return new() { IsHealthy = isHealthy };
		}
	}
}
