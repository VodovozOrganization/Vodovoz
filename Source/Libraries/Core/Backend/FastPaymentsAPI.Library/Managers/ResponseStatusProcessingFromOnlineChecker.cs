using System;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Responses;
using FastPaymentsAPI.Library.Models;
using FastPaymentsAPI.Library.Notifications;
using Microsoft.Extensions.Logging;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Library.Managers
{
	public class ResponseStatusProcessingFromOnlineChecker : FastPaymentStatusCheckerBase
	{
		private readonly ILogger<ResponseStatusProcessingFromOnlineChecker> _logger;
		private readonly IFastPaymentService _fastPaymentService;
		private readonly SiteNotifier _siteNotifier;
		private readonly MobileAppNotifier _mobileAppNotifier;

		public ResponseStatusProcessingFromOnlineChecker(
			ILogger<ResponseStatusProcessingFromOnlineChecker> logger,
			IFastPaymentService fastPaymentService,
			SiteNotifier siteNotifier,
			MobileAppNotifier mobileAppNotifier)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastPaymentService = fastPaymentService ?? throw new ArgumentNullException(nameof(fastPaymentService));
			_siteNotifier = siteNotifier ?? throw new ArgumentNullException(nameof(siteNotifier));
			_mobileAppNotifier = mobileAppNotifier ?? throw new ArgumentNullException(nameof(mobileAppNotifier));
		}
		
		public override async Task<(bool NeedReturnResult, IFastPaymentStatusDto Result)> CheckStatus(
			IFastPaymentStatusDto result, FastPayment fastPayment, OrderInfoResponseDTO response)
		{
			if(response.Status == FastPaymentDTOStatus.Processing)
			{
				_logger.LogInformation("Отменяем платеж с сессией {Ticket}", fastPayment.Ticket);
				_fastPaymentService.UpdateFastPaymentStatus(fastPayment, FastPaymentDTOStatus.Rejected, DateTime.Now);
				
				await _siteNotifier.NotifyPaymentStatusChangeAsync(fastPayment);
				await _mobileAppNotifier.NotifyPaymentStatusChangeAsync(fastPayment);
			}
			
			if(NextHandler != null)
			{
				return await NextHandler.CheckStatus(result, fastPayment, response);
			}
			
			return (false, result);
		}
	}
}
