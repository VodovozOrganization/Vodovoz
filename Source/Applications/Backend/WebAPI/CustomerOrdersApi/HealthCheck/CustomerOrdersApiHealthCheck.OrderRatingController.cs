using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using Vodovoz.Core.Domain.Clients;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;

namespace CustomerOrdersApi.HealthCheck
{
	public partial class CustomerOrdersApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckOrderRatingController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Регистрация оценки заказа",
					checkMethodName => CheckCreateOrderRating(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Получение всех причин оценки заказа",
					checkMethodName => CheckGetOrderRatingReasons(checkMethodName, cancellationToken))
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckCreateOrderRating(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("CreateOrderRating").Get<OrderRatingInfoForCreateDto>();

			var result = await HttpResponseHelper.SendRequestAsync<string>(
				HttpMethod.Post,
				$"{_baseAddress}/api/CreateOrderRating",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			return VodovozHealthResultDto.FromCondition(checkMethodName, result.IsSuccess, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckGetOrderRatingReasons(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestParameter = _healthSection.GetSection("GetOrderRatingReasonsSource").Get<Source>();

			var result = await HttpResponseHelper.SendRequestAsync<IEnumerable<OrderRatingReasonDto>>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetOrderRatingReasons?source={requestParameter}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.IsSuccess && result.Data.Any();

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}
	}
}
