using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using FastPaymentsApi.Contracts;
using FastPaymentsAPI.Library.Models;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	public class FastPaymentRefundService : PaymentRefundServiceBase
	{
		private readonly IFastPaymentOrderService _fastPaymentOrderService;
		private readonly IFastPaymentService _fastPaymentService;

		public FastPaymentRefundService(
			ILogger<FastPaymentRefundService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHttpClientFactory httpClientFactory,
			IFastPaymentOrderService fastPaymentOrderService,
			IFastPaymentService fastPaymentService,
			IRefundOperationRepository refundOperationRepository
		) : base(logger, unitOfWorkFactory, httpClientFactory, refundOperationRepository)
		{
			_fastPaymentOrderService = fastPaymentOrderService ?? throw new ArgumentNullException(nameof(fastPaymentOrderService));
			_fastPaymentService = fastPaymentService ?? throw new ArgumentNullException(nameof(fastPaymentService));
		}

		public override bool CanHandle(OnlinePaymentSource paymentSource)
			=> paymentSource is OnlinePaymentSource.FromMobileAppByQr or OnlinePaymentSource.FromVodovozWebSiteByQr;

		protected override async Task<RefundResultDto> ProcessRefundInternalAsync(RefundRequestDto request, string idempotenceKey, CancellationToken cancellationToken)
		{
			var ticket = request.TransactionId;

			_logger.LogInformation(
				"Начало отмены платежа QR для заказа {ExternalOrderId}, Ticket: {Ticket}",
				request.ExternalOrderId,
				ticket);

			_logger.LogInformation("Пришел запрос на отмену платежа с сессией: {Ticket}", ticket);
			var fastPayment = _fastPaymentService.GetFastPaymentByTicket(ticket);

			if(fastPayment == null)
			{
				_logger.LogError("Платеж с сессией: {Ticket} не найден в базе", ticket);
				return CreateErrorResult("Идентификатор платежа не найден");
			}

			_logger.LogInformation("Посылаем запрос в банк на отмену сессии оплаты: {Ticket}", ticket);

			var cancelResponse = await _fastPaymentOrderService.CancelPayment(ticket, fastPayment.Organization);

			if(cancelResponse.ResponseCode != 0)
			{
				return CreateErrorResult($"Ошибка отмены платежа: {ticket}");
			}

			_logger.LogInformation("Обновляем статус платежа");
			_fastPaymentService.UpdateFastPaymentStatus(fastPayment, FastPaymentDTOStatus.Rejected, DateTime.Now);

			_logger.LogInformation(
				"Платеж QR успешно отменен для заказа {ExternalOrderId}",
				request.ExternalOrderId);

			return CreateSuccessResult(ticket);
		}
	}
}
