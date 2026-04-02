using CloudPaymentsApi.Client;
using CloudPaymentsApi.Library.Models;
using CustomerOrdersApi.Library.Default.Services.PaymentRefund;
using CustomerOrdersApi.Library.Default.Services.PaymentRefund.Mappers;
using CustomerOrdersApi.Library.V4.Dto.Orders.CancelOrder;
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
	public class CloudPaymentsRefundService : PaymentRefundServiceBase
	{
		private readonly ICloudPaymentsApiClient _cloudPaymentsClient;
		private readonly ICloudPaymentsMapper _mapper;

		public CloudPaymentsRefundService(
			ILogger<CloudPaymentsRefundService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICloudPaymentsApiClient cloudPaymentsClient,
			ICloudPaymentsMapper mapper,
			IHttpClientFactory httpClientFactory,
			IRefundOperationRepository refundOperationRepository
			) : base(logger, unitOfWorkFactory, httpClientFactory, refundOperationRepository)
		{
			_cloudPaymentsClient = cloudPaymentsClient ?? throw new ArgumentNullException(nameof(cloudPaymentsClient));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		public override bool CanHandle(OnlinePaymentSource paymentSource) => paymentSource is OnlinePaymentSource.FromMobileApp;

		protected override async Task<RefundResultDto> ProcessRefundInternalAsync(RefundRequestDto request, string idempotenceKey, CancellationToken cancellationToken)
		{
			if(!long.TryParse(request.TransactionId, out var transactionId))
			{
				_logger.LogWarning("Неверный формат TransactionId: {TransactionId}", request.TransactionId);
				return CreateErrorResult($"Неверный формат идентификатора транзакции: {request.TransactionId}");
			}

			var transactionResponse = await _cloudPaymentsClient.GetTransactionAsync(transactionId, cancellationToken);

			if(!transactionResponse.Success)
			{
				return CreateErrorResult($"Не удалось получить транзакцию: {transactionResponse.Message}");
			}

			var transaction = transactionResponse.Model;

			if(transaction.Refunded is true || transaction.Type is CloudPaymentsOperationType.Refund)
			{
				_logger.LogWarning("Попытка повторного возврата по транзакции {TransactionId}", request.TransactionId);
				return CreateErrorResult("Возврат уже был выполнен");
			}

			var refundDto = _mapper.MapToRefundRequest(request);
			var refundResponse = await _cloudPaymentsClient.RefundAsync(refundDto, idempotenceKey, cancellationToken);

			if(!refundResponse.Success)
			{
				return CreateErrorResult($"Ошибка возврата: {refundResponse.Message}");
			}

			return _mapper.MapToRefundResult(refundResponse);
		}
	}
}
