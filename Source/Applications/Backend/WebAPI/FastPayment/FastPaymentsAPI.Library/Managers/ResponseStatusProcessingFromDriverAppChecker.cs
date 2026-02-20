using System;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Responses;
using FastPaymentsAPI.Library.Models;
using Microsoft.Extensions.Logging;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Library.Managers
{
	public class ResponseStatusProcessingFromDriverAppChecker : FastPaymentStatusCheckerBase
	{
		private readonly ILogger<ResponseStatusProcessingFromDriverAppChecker> _logger;
		private readonly IFastPaymentOrderService _fastPaymentOrderService;

		public ResponseStatusProcessingFromDriverAppChecker(
			ILogger<ResponseStatusProcessingFromDriverAppChecker> logger,
			IFastPaymentOrderService fastPaymentOrderService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastPaymentOrderService = fastPaymentOrderService ?? throw new ArgumentNullException(nameof(fastPaymentOrderService));
		}
		
		public override async Task<(bool NeedReturnResult, IFastPaymentStatusDto Result)> CheckStatus(
			IFastPaymentStatusDto result, FastPayment fastPayment, OrderInfoResponseDTO response)
		{
			if(response.Status == FastPaymentDTOStatus.Processing)
			{
				if(result is not QRResponseDTO qrResult)
				{
					result.ErrorMessage = "Ошибка сервера. Обратитесь в техподдержку";
					_logger.LogError("Ошибка приведения. Не удалось привести result к {Dto}", nameof(QRResponseDTO));
					return (true, result);
				}
				
				var order = _fastPaymentOrderService.GetOrder(fastPayment.Order.Id);
				
				if(!string.IsNullOrWhiteSpace(fastPayment.QRPngBase64)
					&& fastPayment.Amount == order.OrderSum)
				{
					qrResult.QRCode = fastPayment.QRPngBase64;
					qrResult.FastPaymentStatus = fastPayment.FastPaymentStatus;
					return (true, qrResult);
				}
			}
			
			if(NextHandler != null)
			{
				return await NextHandler.CheckStatus(result, fastPayment, response);
			}
			
			return (false, result);
		}
	}
}
