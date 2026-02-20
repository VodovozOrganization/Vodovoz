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
		private async Task<VodovozHealthResultDto> CheckDiscountController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Проверка возможности применения и применение промокода",
					checkMethodName => CheckApplyPromoCodeToOrder(checkMethodName, cancellationToken)),

				ExecuteHealthCheckSafelyAsync("Оповещение пользователя о применимости промокода",
					checkMethodName => CheckGetPromoCodeWarningMessage(checkMethodName, cancellationToken))
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckApplyPromoCodeToOrder(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("ApplyPromoCodeToOrder").Get<ApplyPromoCodeDto>();

			// Пока нет в БД основания скидки - промокод
			// var result = await ResponseHelper.GetByUriWithBody<ApplyPromoCodeDto, IEnumerable<IOnlineOrderedProduct>>(
			// 	$"{_baseAddress}/api/ApplyPromoCodeToOrder",
			// 	_httpClientFactory,
			// 	requestDto,
			// 	cancellationToken);

			return VodovozHealthResultDto.HealthyResult();
		}

		private async Task<VodovozHealthResultDto> CheckGetPromoCodeWarningMessage(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("GetPromoCodeWarningMessage").Get<PromoCodeWarningDto>();

			var result = await HttpResponseHelper.SendRequestAsync<string>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetPromoCodeWarningMessage",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			return VodovozHealthResultDto.FromCondition(checkMethodName, result.IsSuccess, result.ErrorMessage);
		}
	}
}
