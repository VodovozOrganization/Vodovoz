using System;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Responses;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentProcessingStatusChecker : FastPaymentStatusCheckerBase
	{
		private readonly ILogger<FastPaymentProcessingStatusChecker> _logger;
		private readonly IOrderRequestManager _orderRequestManager;
		private const string _errorMessageTemplate = "При получении информации о сессии {0} произошла ошибка";

		public FastPaymentProcessingStatusChecker(
			ILogger<FastPaymentProcessingStatusChecker> logger,
			IOrderRequestManager orderRequestManager)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_orderRequestManager = orderRequestManager ?? throw new ArgumentNullException(nameof(orderRequestManager));
		}

		public override async Task<(bool NeedReturnResult, IFastPaymentStatusDto Result)> CheckStatus(
			IFastPaymentStatusDto result, FastPayment fastPayment)
		{
			var ticket = fastPayment.Ticket;
			
			if(fastPayment.FastPaymentStatus == FastPaymentStatus.Processing)
			{
				_logger.LogInformation("Делаем запрос в банк, чтобы узнать статус оплаты сессии {Ticket}", ticket);
				var orderInfoResponseDto = await _orderRequestManager.GetOrderInfo(ticket, fastPayment.Organization);

				if(orderInfoResponseDto.ResponseCode != 0)
				{
					result.ErrorMessage = string.Format(_errorMessageTemplate, ticket);
					var logMessage = string.Format(_errorMessageTemplate, "{Ticket}");
					
					_logger.LogError(logMessage + " Код ответа {ResponseCode}\n{ResponseMessage}",
						ticket,
						orderInfoResponseDto.ResponseCode,
						orderInfoResponseDto.ResponseMessage);
					
					return (true, result);
				}
				
				if(NextHandler != null)
				{
					return await NextHandler.CheckStatus(result, fastPayment, orderInfoResponseDto);
				}
			}

			return (false, result);
		}
	}
}
