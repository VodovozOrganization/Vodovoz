using FastPaymentsApi.Contracts.Requests;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace FastPaymentsAPI.HealthChecks
{
	public partial class FastPaymentsHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckPaymentStatusController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync(
					"Получение информации об оплате онлайн-заказа с сайта или мобильного приложения с сохранением уведомления о быстрых платежах в БД",
					checkMethodName => CheckGetPaymentStatus(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Получение информации об оплате онлайн-заказа с сайта или мобильного приложения",
					checkMethodName => CheckGetCheckPaymentStatus(checkMethodName, cancellationToken))
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckGetPaymentStatus(string checkMethodName, CancellationToken cancellationToken)
		{
			var getPaymentStatusSection = _healthSection.GetSection("GetPaymentStatus");
			var orderId = getPaymentStatusSection.GetValue<int>("OrderId");
			var paymentSum = getPaymentStatusSection.GetValue<decimal>("ResponsePaymentSum");


			var result = await HttpResponseHelper.SendRequestAsync<FastPaymentStatusDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetPaymentStatus?orderId={orderId}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.Data != null
				&& result.Data.PaymentStatus == RequestPaymentStatus.Performed
				&& result.Data.PaymentDetails?.PaymentSumDetails?.PaymentSum == paymentSum;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckGetCheckPaymentStatus(string checkMethodName, CancellationToken cancellationToken)
		{
			var getCheckPaymentStatusSection = _healthSection.GetSection("GetCheckPaymentStatus");
			var orderId = getCheckPaymentStatusSection.GetValue<int>("OrderId");
			var paymentSum = getCheckPaymentStatusSection.GetValue<decimal>("ResponsePaymentSum");

			var result = await HttpResponseHelper.SendRequestAsync<FastPaymentStatusDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetCheckPaymentStatus?orderId={orderId}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.Data != null
				&& result.Data.PaymentStatus == RequestPaymentStatus.Performed
				&& result.Data.PaymentDetails?.PaymentSumDetails?.PaymentSum == paymentSum;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}
	}
}
