using DeliveryRulesService.DTO;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace DeliveryRulesService.HealthChecks
{
	public class DeliveryRulesServiceHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public DeliveryRulesServiceHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			var healthResult = new VodovozHealthResultDto();

			var httpClient = _httpClientFactory.CreateClient();
			httpClient.BaseAddress = new Uri($"{baseAddress}");
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var responseDeliveryInfo = await httpClient.GetAsync("GetDeliveryInfo?latitude=59.886134&longitude=30.394007");
			var responseDeliveryInfoBody = await responseDeliveryInfo.Content.ReadAsStreamAsync();
			var responseDeliveryInfoResult = await JsonSerializer.DeserializeAsync<DeliveryInfoDTO>(responseDeliveryInfoBody);
			var deliveryInfoIsHealthy = responseDeliveryInfoResult?.StatusEnum != DeliveryRulesResponseStatus.Error;

			if(!deliveryInfoIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("Не пройдена проверка GetDeliveryInfo.");
			}

			var responseRulesByDistrict = await httpClient.GetAsync("GetRulesByDistrict?latitude=59.886134&longitude=30.394007");
			var responseRulesByDistrictBody = await responseRulesByDistrict.Content.ReadAsStreamAsync();
			var responseRulesByDistrictResult = await JsonSerializer.DeserializeAsync<DeliveryInfoDTO>(responseRulesByDistrictBody);
			var rulesByDistrictIsHealthy = responseRulesByDistrictResult?.StatusEnum != DeliveryRulesResponseStatus.Error;

			if(!rulesByDistrictIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("Не пройдена проверка GetRulesByDistrict.");
			}

			healthResult.IsHealthy = deliveryInfoIsHealthy && rulesByDistrictIsHealthy;

			return healthResult;
		}
	}
}
