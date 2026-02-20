using System;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Responses;
using FastPaymentsAPI.Library.Models;
using Microsoft.Extensions.Logging;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Library.Managers
{
	public class ResponseStatusProcessingFromOnlineChecker : FastPaymentStatusCheckerBase
	{
		private readonly ILogger<ResponseStatusProcessingFromOnlineChecker> _logger;
		private readonly IFastPaymentService _fastPaymentService;

		public ResponseStatusProcessingFromOnlineChecker(
			ILogger<ResponseStatusProcessingFromOnlineChecker> logger,
			IFastPaymentService fastPaymentService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastPaymentService = fastPaymentService ?? throw new ArgumentNullException(nameof(fastPaymentService));
		}
		
		public override async Task<(bool NeedReturnResult, IFastPaymentStatusDto Result)> CheckStatus(
			IFastPaymentStatusDto result, FastPayment fastPayment, OrderInfoResponseDTO response)
		{
			if(response.Status == FastPaymentDTOStatus.Processing)
			{
				_logger.LogInformation("Отменяем платеж с сессией {Ticket}", fastPayment.Ticket);
				_fastPaymentService.UpdateFastPaymentStatus(fastPayment, FastPaymentDTOStatus.Rejected, DateTime.Now);
			}
			
			if(NextHandler != null)
			{
				return await NextHandler.CheckStatus(result, fastPayment, response);
			}
			
			return (false, result);
		}
	}
}
