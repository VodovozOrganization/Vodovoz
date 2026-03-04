using CustomerAppsApi.Library.Dto.Counterparties;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.HealthChecks
{
	public partial class CustomerAppsApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckCounterpartyController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Получение информации о пользователе",
					checkMethodName => CheckGetCounterparty(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Регистрация пользователя",
					checkMethodName => CheckRegisterCounterparty(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Обновление информации о пользователе",
					checkMethodName => CheckUpdateCounterpartyInfo(checkMethodName, cancellationToken))
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckGetCounterparty(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("GetCounterparty").Get<CounterpartyContactInfoDto>();

			var result = await HttpResponseHelper.SendRequestAsync<CounterpartyIdentificationDto>(
				HttpMethod.Post,
				$"{_baseAddress}/api/GetCounterparty",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			var isHealthy = result.Data?.CounterpartyIdentificationStatus == CounterpartyIdentificationStatus.Success;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.Data?.ErrorDescription ?? result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckRegisterCounterparty(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("RegisterCounterparty").Get<CounterpartyDto>();

			var result = await HttpResponseHelper.SendRequestAsync<CounterpartyRegistrationDto>(
				HttpMethod.Post,
				$"{_baseAddress}/api/RegisterCounterparty",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			var isHealthy = result.Data?.CounterpartyRegistrationStatus == CounterpartyRegistrationStatus.CounterpartyRegistered;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.Data?.ErrorDescription ?? result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckUpdateCounterpartyInfo(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("UpdateCounterpartyInfo").Get<CounterpartyDto>();

			var result = await HttpResponseHelper.SendRequestAsync<CounterpartyUpdateDto>(
				HttpMethod.Post,
				$"{_baseAddress}/api/UpdateCounterpartyInfo",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			var isHealthy = result.Data?.CounterpartyUpdateStatus == CounterpartyUpdateStatus.CounterpartyUpdated;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.Data?.ErrorDescription ?? result.ErrorMessage);
		}
	}
}
