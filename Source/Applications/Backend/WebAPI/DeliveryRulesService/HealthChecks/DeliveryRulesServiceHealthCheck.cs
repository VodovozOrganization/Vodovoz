using DeliveryRulesService.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
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

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			var healthResult = new VodovozHealthResultDto();

			var deliveryInfo = await HttpResponseHelper.GetJsonByUriAsync<DeliveryInfoDTO>(
				$"{baseAddress}/DeliveryRules/GetDeliveryInfo?latitude=59.886134&longitude=30.394007",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var deliveryInfoIsHealthy = deliveryInfo?.StatusEnum != DeliveryRulesResponseStatus.Error;

			if(!deliveryInfoIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("Не пройдена проверка GetDeliveryInfo.");
			}

			var rulesByDistrict = await HttpResponseHelper.GetJsonByUriAsync<DeliveryInfoDTO>(
				$"{baseAddress}/DeliveryRules/GetRulesByDistrict?latitude=59.886134&longitude=30.394007",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var rulesByDistrictIsHealthy = rulesByDistrict?.StatusEnum != DeliveryRulesResponseStatus.Error;

			if(!rulesByDistrictIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("Не пройдена проверка GetRulesByDistrict.");
			}

			var getRulesByDistrictAndNomenclaturesRequest = healthSection.GetSection("GetRulesByDistrictAndNomenclatures").Get<DeliveryRulesRequest>();

			var getRulesByDistrictAndNomenclaturesResult = await HttpResponseHelper.SendRequestAsync<DeliveryRulesDTO>(
				HttpMethod.Post,
				$"{baseAddress}/DeliveryRules/GetRulesByDistrictAndNomenclatures",
				_httpClientFactory,
				getRulesByDistrictAndNomenclaturesRequest.ToJsonContent(),
				cancellationToken: cancellationToken);

			var getRulesByDistrictAndNomenclaturesIsHealthy = getRulesByDistrictAndNomenclaturesResult.Data?.StatusEnum == DeliveryRulesResponseStatus.Ok;

			var getExtendedRulesByDistrictAndNomenclaturesRequest = healthSection.GetSection("GetExtendedRulesByDistrictAndNomenclatures").Get<DeliveryRulesRequest>();

			var getExtendedRulesByDistrictAndNomenclaturesResult = await HttpResponseHelper.SendRequestAsync<ExtendedDeliveryRulesDto>(
				HttpMethod.Post,
				$"{baseAddress}/DeliveryRules/GetExtendedRulesByDistrictAndNomenclatures",
				_httpClientFactory,
				getExtendedRulesByDistrictAndNomenclaturesRequest.ToJsonContent(),
				cancellationToken: cancellationToken);

			var geExtendedRulesByDistrictAndNomenclaturesIsHealthy = getExtendedRulesByDistrictAndNomenclaturesResult.Data?.StatusEnum == DeliveryRulesResponseStatus.Ok;

			healthResult.IsHealthy = deliveryInfoIsHealthy
									 && rulesByDistrictIsHealthy
									 && getRulesByDistrictAndNomenclaturesIsHealthy
									 && geExtendedRulesByDistrictAndNomenclaturesIsHealthy;

			return healthResult;
		}
	}
}
