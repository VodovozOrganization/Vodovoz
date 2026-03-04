using CustomerAppsApi.Library.Dto;
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
		private async Task<VodovozHealthResultDto> CheckRentPackagesController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Получение бесплатных пакетов аренды",
					checkMethodName => GetFreeRentPackages(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> GetFreeRentPackages(string checkMethodName, CancellationToken cancellationToken)
		{
			var source = _healthSection.GetSection("GetFreeRentPackagesSource").Get<string>();

			var result = await HttpResponseHelper.SendRequestAsync<FreeRentPackagesDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetFreeRentPackages?source={source}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.Data?.RentPackages?.Any() ?? false;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage ?? result.Data?.ErrorMessage);
		}
	}
}
