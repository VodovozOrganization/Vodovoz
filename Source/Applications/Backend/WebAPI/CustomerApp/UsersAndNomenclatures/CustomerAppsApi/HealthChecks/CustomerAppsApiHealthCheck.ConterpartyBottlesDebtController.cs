using CustomerAppsApi.Library.Dto.Counterparties;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.HealthChecks
{
	public partial class CustomerAppsApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckCounterpartyBottlesDebtController(CancellationToken cancellationToken)
		{
			var chekcs = new[]
			{
				ExecuteHealthCheckSafelyAsync("Получение долга по бутылям контрагента",
					checkMethodName => CheckGetCounterpartyBottlesDebt(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(chekcs);
		}

		private async Task<VodovozHealthResultDto> CheckGetCounterpartyBottlesDebt(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestParameter = _healthSection.GetSection("GetCounterpartyBottlesDebtErpCounterpartyId").Get<int>();

			var result = await HttpResponseHelper.SendRequestAsync<CounterpartyBottlesDebtDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetCounterpartyBottlesDebt?erpCounterpartyId={requestParameter}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.Data?.CounterpartyBottlesDebt > 0;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}
	}
}
