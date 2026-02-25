using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Requests;
using FastPaymentsApi.Contracts.Responses;
using FastPaymentsApi.Contracts.Responses.OnlineOrderRegistration;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.FastPayments;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;

namespace FastPaymentsAPI.HealthChecks
{
	public partial class FastPaymentsHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckFastPaymentsController(CancellationToken cancellationToken)
		{
			var checks = new[]
			{
				ExecuteHealthCheckSafelyAsync("Проверка возможности применения и применение промокода",
					checkMethodName => CheckRegisterOrderForGetQR(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync(
					"Регистрация заказа в системе эквайринга и получения сессии оплаты для формирования ссылки на оплату для ДВ",
					checkMethodName => CheckRegisterOrder(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Регистрация онлайн-заказа с сайта и получения ссылки на платежную страницу",
					checkMethodName => CheckRegisterOnlineOrder(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Регистрация онлайн-заказа мобильного приложения и получения ссылки на платежную страницу",
					checkMethodName => CheckRegisterOnlineOrderFromMobileApp(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Получение информации об оплаченном заказе и уведомление водителя",
					checkMethodName => CheckReceivePayment(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Отмена сессии оплаты/платежа",
					checkMethodName => CheckCancelPayment(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Получение информации об оплате",
					checkMethodName => CheckGetOrderInfo(checkMethodName, cancellationToken)),
				ExecuteHealthCheckSafelyAsync("Для проверки работы сервиса - получение заказа по id",
					checkMethodName => CheckGetOrderId(checkMethodName, cancellationToken))
			};

			return await ConcatHealthCheckResultsAsync(checks);
		}

		private async Task<VodovozHealthResultDto> CheckRegisterOrderForGetQR(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("RegisterOrderForGetQR").Get<OrderDTO>();

			var result = await HttpResponseHelper.SendRequestAsync<QRResponseDTO>(
				HttpMethod.Post,
				$"{_baseAddress}/api/RegisterOrderForGetQR",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			var isHealthy = result.IsSuccess && result.Data is { FastPaymentStatus: FastPaymentStatus.Processing, QRCode: "HealthCheck" };

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckRegisterOrder(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("RegisterOrder").Get<FastPaymentRequestDTO>();

			var result = await HttpResponseHelper.SendRequestAsync<FastPaymentResponseDTO>(
				HttpMethod.Post,
				$"{_baseAddress}/api/RegisterOrder",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			var isHealthy = result.IsSuccess && result.Data is { FastPaymentStatus: FastPaymentStatus.Processing, Ticket: "HealthCheck" };

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckRegisterOnlineOrder(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("RegisterOnlineOrder").Get<RequestRegisterOnlineOrderDTO>();

			var result = await HttpResponseHelper.SendRequestAsync<ResponseRegisterOnlineOrder>(
				HttpMethod.Post,
				$"{_baseAddress}/api/RegisterOnlineOrder",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			var isHealthy = result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data?.PayUrl);

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckRegisterOnlineOrderFromMobileApp(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("RegisterOnlineOrderFromMobileApp").Get<RequestRegisterOnlineOrderDTO>();

			var result = await HttpResponseHelper.SendRequestAsync<ResponseRegisterOnlineOrder>(
				HttpMethod.Post,
				$"{_baseAddress}/api/RegisterOnlineOrderFromMobileApp",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			var isHealthy = result.IsSuccess && result.Data?.QrCode == "HealthCheck";

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}


		private async Task<VodovozHealthResultDto> CheckReceivePayment(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("ReceivePayment").Get<PaidOrderDTO>();

			var result = await HttpResponseHelper.SendRequestAsync<string>(
				HttpMethod.Post,
				$"{_baseAddress}/api/ReceivePayment",
				_httpClientFactory,
				requestDto.ToFormUrlEncodedContent(),
				cancellationToken);

			return VodovozHealthResultDto.FromCondition(checkMethodName, result.IsSuccess, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckCancelPayment(string checkMethodName, CancellationToken cancellationToken)
		{
			var requestDto = _healthSection.GetSection("CancelPayment").Get<CancelTicketRequestDTO>();

			var result = await HttpResponseHelper.SendRequestAsync<CancelTicketResponseDTO>(
				HttpMethod.Post,
				$"{_baseAddress}/api/CancelPayment",
				_httpClientFactory,
				requestDto.ToJsonContent(),
				cancellationToken);

			var isHealthy = result.Data?.ResponseStatus == ResponseStatus.Success;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.Data?.ErrorMessage ?? result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckGetOrderInfo(string checkMethodName, CancellationToken cancellationToken)
		{
			var ticket = _healthSection.GetSection("GetOrderInfoTicket").Get<string>();

			var result = await HttpResponseHelper.SendRequestAsync<OrderInfoResponseDTO>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetOrderInfo?ticket={ticket}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.Data is { Status: FastPaymentDTOStatus.Performed, Id: 28515187 };

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.Data?.ResponseMessage ?? result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckGetOrderId(string checkMethodName, CancellationToken cancellationToken)
		{
			var orderId = _healthSection.GetSection("GetOrderId").Get<int>();

			var result = await HttpResponseHelper.SendRequestAsync<OrderDTO>(
				HttpMethod.Get,
				$"{_baseAddress}/api/GetOrderId?orderId={orderId}",
				_httpClientFactory,
				cancellationToken: cancellationToken);

			var isHealthy = result.Data?.OrderId == orderId;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}
	}
}
