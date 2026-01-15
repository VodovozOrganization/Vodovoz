using CustomerOrdersApi.Library.Dto.Orders.FixedPrice;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Nodes;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;

namespace CustomerOrdersApi.HealthCheck
{
	public partial class CustomerOrdersApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckFixedPriceController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Применение фиксы",
					checkMethodName => CheckApplyFixedPriceToOrder(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckApplyFixedPriceToOrder(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("ApplyFixedPriceToOrder").Get<ApplyFixedPriceDto>();

			var jsonSerializerOptions = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				Converters =
				{
					new InterfaceToImplementationJsonConverter<IOnlineOrderedProductWithFixedPrice, OnlineOrderItemWithFixedPrice>()
				}
			};

			var result = await HttpResponseHelper.SendRequestAsync<AppliedFixedPriceDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/ApplyFixedPriceToOrder",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken,
				jsonSerializerOptions: jsonSerializerOptions);

			var product = result.Data?.OnlineOrderItems?.FirstOrDefault();
			var isHealthy = product?.OldPrice != product?.NewPrice;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}
	}
}
