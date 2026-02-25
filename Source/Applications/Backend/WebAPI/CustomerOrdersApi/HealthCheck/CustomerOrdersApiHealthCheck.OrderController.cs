using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;

namespace CustomerOrdersApi.HealthCheck
{
	public partial class CustomerOrdersApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckOrderController(CancellationToken cancellationToken)
		{
			var cheks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Регистрация заказа",
					checkMethodName => CheckCreateOrder(checkMethodName, cancellationToken)),

				ExecuteHealthCheckSafelyAsync("Получение детальной информации о заказе",
					checkMethodName => CheckGetOrderInfo(checkMethodName, cancellationToken)),

				ExecuteHealthCheckSafelyAsync("Получение заказов клиента",
					checkMethodName => CheckGetOrders(checkMethodName, cancellationToken))
			};

			return await ConcatHealthCheckResultsAsync(cheks);
		}

		private async Task<VodovozHealthResultDto> CheckCreateOrder(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("CreateOrder").Get<OnlineOrderInfoDto>();

			var result = await HttpResponseHelper.SendRequestAsync<string>(
				HttpMethod.Post,
				$"{_baseAddress}/api/CreateOrder",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			return VodovozHealthResultDto.FromCondition(
				checkMethodName,
				result.IsSuccess,
				result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckGetOrderInfo(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("GetOrderInfo").Get<GetDetailedOrderInfoDto>();

			var result = await HttpResponseHelper.SendRequestAsync<DetailedOrderInfoDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetOrderInfo",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			return VodovozHealthResultDto.FromCondition(
				checkMethodName,
				requestDto.OrderId == result.Data?.OrderId,
				result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckGetOrders(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("GetOrders").Get<GetOrdersDto>();

			var result = await HttpResponseHelper.SendRequestAsync<OrdersDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetOrders",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			return VodovozHealthResultDto.FromCondition(checkMethodName, result.Data?.OrdersCount > 0, result.ErrorMessage);
		}
	}
}
