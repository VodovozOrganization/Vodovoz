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
		private readonly IConfigurationSection _healthSection;
		private readonly string _baseAddress;

		public DeliveryRulesServiceHealthCheck(
			ILogger<DeliveryRulesServiceHealthCheck> logger,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory)
			: base(logger, unitOfWorkFactory)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_healthSection = _configuration.GetSection("Health");
			_baseAddress = _healthSection.GetValue<string>("BaseAddress");
		}

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			var checks = new[]
{
				ExecuteHealthCheckSafelyAsync("Получение графиков доставки по координатам",
					checkMethodName => GetDeliveryInfo(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Получение правил доставки с графиками и ценами по координатам",
					checkMethodName => GetRulesByDistrict(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Получение правил доставки по координатам и номенклатурам",
					checkMethodName => GetRulesByDistrictAndNomenclatures(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Получение правил доставки с ДЗЧ по координатам и номенклатурам",
					checkMethodName => GetExtendedRulesByDistrictAndNomenclatures(checkMethodName, cancellationToken))
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> GetExtendedRulesByDistrictAndNomenclatures(string checkMethodName, CancellationToken cancellationToken)
		{
			var getExtendedRulesByDistrictAndNomenclaturesRequest = _healthSection.GetSection("GetExtendedRulesByDistrictAndNomenclatures").Get<DeliveryRulesRequest>();

			var result = await HttpResponseHelper.SendRequestAsync<ExtendedDeliveryRulesDto>(
				HttpMethod.Post,
				$"{_baseAddress}/DeliveryRules/GetExtendedRulesByDistrictAndNomenclatures",
				_httpClientFactory,
				getExtendedRulesByDistrictAndNomenclaturesRequest.ToJsonContent(),
				cancellationToken: cancellationToken);

			var isHealthy = result.Data?.StatusEnum == DeliveryRulesResponseStatus.Ok;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> GetRulesByDistrictAndNomenclatures(string checkMethodName, CancellationToken cancellationToken)
		{
			var getRulesByDistrictAndNomenclaturesRequest = _healthSection.GetSection("GetRulesByDistrictAndNomenclatures").Get<DeliveryRulesRequest>();

			var result = await HttpResponseHelper.SendRequestAsync<DeliveryRulesDTO>(
				HttpMethod.Post,
				$"{_baseAddress}/DeliveryRules/GetRulesByDistrictAndNomenclatures",
				_httpClientFactory,
				getRulesByDistrictAndNomenclaturesRequest.ToJsonContent(),
				cancellationToken: cancellationToken);

			var isHealthy = result.Data?.StatusEnum == DeliveryRulesResponseStatus.Ok;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> GetRulesByDistrict(string checkMethodName, CancellationToken cancellationToken)
		{
			var result = await HttpResponseHelper.SendRequestAsync<(int? TariffZoneId, DeliveryRulesDTO DeliveryInfo)>(
				HttpMethod.Get,
				$"{_baseAddress}/DeliveryRules/GetRulesByDistrict?latitude=59.886134&longitude=30.394007",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.IsSuccess;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> GetDeliveryInfo(string checkMethodName, CancellationToken cancellationToken)
		{

			var healthResult = new VodovozHealthResultDto();

			var result = await HttpResponseHelper.SendRequestAsync<DeliveryInfoDTO>(
				HttpMethod.Get,
				$"{_baseAddress}/DeliveryRules/GetDeliveryInfo?latitude=59.886134&longitude=30.394007",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result?.Data.StatusEnum != DeliveryRulesResponseStatus.Error;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}
	}
}
