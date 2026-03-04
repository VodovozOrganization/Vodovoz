using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Store;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.HealthChecks
{
	public partial class CustomerAppsApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckWarehouseController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Выборка адресов самовывоза",
					checkMethodName => CheckGetSelfDeliveriesAddresses(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckGetSelfDeliveriesAddresses(string checkMethodName, CancellationToken cancellationToken)
		{
			var source = _healthSection.GetSection("GetSelfDeliveriesAddressesSource").Get<string>();

			var result = await HttpResponseHelper.SendRequestAsync<IEnumerable<SelfDeliveryAddressDto>>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetSelfDeliveriesAddresses?source={source}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.Data?.Any() ?? false;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}
	}
}
