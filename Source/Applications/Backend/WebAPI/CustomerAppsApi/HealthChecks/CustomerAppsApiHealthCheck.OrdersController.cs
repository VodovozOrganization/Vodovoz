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
		private async Task<VodovozHealthResultDto> CheckOrdersController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Может ли клиент заказывать промонаборы для новых клиентов",
					checkMethodName => CanCounterpartyOrderPromoSetForNewClients(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CanCounterpartyOrderPromoSetForNewClients(string checkMethodName,
			CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("CanCounterpartyOrderPromoSetForNewClients").Get<FreeLoaderCheckingDto>();

			var result = await HttpResponseHelper.SendRequestAsync<bool>(
				HttpMethod.Get,
				$"{_baseAddress}/api/CanCounterpartyOrderPromoSetForNewClients",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			return VodovozHealthResultDto.FromCondition(checkMethodName, result.Data, result.ErrorMessage);
		}
	}
}
