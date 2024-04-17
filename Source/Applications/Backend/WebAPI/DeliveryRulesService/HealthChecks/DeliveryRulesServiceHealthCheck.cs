using DeliveryRulesService.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace DeliveryRulesService.HealthChecks
{
	public class DeliveryRulesServiceHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public DeliveryRulesServiceHealthCheck(
			ILogger<DeliveryRulesServiceHealthCheck> logger,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory)
			: base(logger, unitOfWorkFactory)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			var healthResult = new VodovozHealthResultDto();

			var deliveryInfo = await ResponseHelper.GetJsonByUri<DeliveryInfoDTO>(
				$"{baseAddress}/DeliveryRules/GetDeliveryInfo?latitude=59.886134&longitude=30.394007", 
				_httpClientFactory);

			var deliveryInfoIsHealthy = deliveryInfo?.StatusEnum != DeliveryRulesResponseStatus.Error;

			if(!deliveryInfoIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("Не пройдена проверка GetDeliveryInfo.");
			}

			var rulesByDistrict = await ResponseHelper.GetJsonByUri<DeliveryInfoDTO>(
				$"{baseAddress}/DeliveryRules/GetRulesByDistrict?latitude=59.886134&longitude=30.394007",
				_httpClientFactory);

			var rulesByDistrictIsHealthy = rulesByDistrict?.StatusEnum != DeliveryRulesResponseStatus.Error;

			if(!rulesByDistrictIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("Не пройдена проверка GetRulesByDistrict.");
			}

			healthResult.IsHealthy = deliveryInfoIsHealthy && rulesByDistrictIsHealthy;

			return healthResult;
		}
	}
}
