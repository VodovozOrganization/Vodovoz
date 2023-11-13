using DeliveryRulesService.DTO;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using VodovozHealthCheck;

namespace DeliveryRulesService.HealthChecks
{
	public class DeliveryRulesServiceHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public DeliveryRulesServiceHealthCheck(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthResult = new VodovozHealthResultDto();

			var httpClient = _httpClientFactory.CreateClient();
			httpClient.BaseAddress = new Uri("https://localhost:44393/DeliveryRules/");
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
