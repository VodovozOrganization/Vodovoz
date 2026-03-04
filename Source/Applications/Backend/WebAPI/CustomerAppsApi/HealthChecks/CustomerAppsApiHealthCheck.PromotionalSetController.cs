using CustomerAppsApi.Library.Dto.Goods;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.HealthChecks
{
	public partial class CustomerAppsApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckPromotionalSetController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Получение списка промонаборов",
					checkMethodName => CheckGetPromotionalSets(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckGetPromotionalSets(string checkMethodName, CancellationToken cancellationToken)
		{
			var source = _healthSection.GetSection("GetPromotionalSetsSource").Get<string>();

			var result = await HttpResponseHelper.SendRequestAsync<PromotionalSetsDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetPromotionalSets?source={source}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.Data?.PromotionalSets?.Any() ?? false;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage ?? result.Data?.ErrorMessage);
		}
	}
}
