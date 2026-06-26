using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Goods;
using Microsoft.Extensions.Configuration;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.V2.HealthChecks
{
	public partial class CustomerAppsApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckNomenclatureController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Получение списка цен номенклатур",
					checkMethodName => CheckGetNomenclaturesPricesAndStocks(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Получение номенклатур, продающихся в ИПЗ",
					checkMethodName => CheckGetNomenclatures(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckGetNomenclaturesPricesAndStocks(string checkMethodName, CancellationToken cancellationToken)
		{
			var source = _healthSection.GetSection("GetNomenclaturesPricesAndStocksSource").Get<string>();

			var result = await HttpResponseHelper.SendRequestAsync<NomenclaturesPricesAndStockDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/{_version}/GetNomenclaturesPricesAndStocks?source={source}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.Data?.PricesAndStocks?.Any() ?? false;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage ?? result.Data?.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckGetNomenclatures(string checkMethodName, CancellationToken cancellationToken)
		{
			var source = _healthSection.GetSection("GetNomenclaturesPricesAndStocksSource").Get<string>();

			var result = await HttpResponseHelper.SendRequestAsync<SaleItemsDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/{_version}/GetNomenclatures?source={source}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.Data?.SaleItems?.Any() ?? false;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage ?? result.Data?.ErrorMessage);
		}
	}
}
