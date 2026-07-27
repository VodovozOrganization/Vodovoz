using CloudPaymentsApi.Client;
using CloudPaymentsApi.Library.Models;
using CustomerOrdersApi.Library.V6.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.V6.Services.PaymentRefund.Mappers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V6.Services.PaymentRefund
{
	public class CloudPaymentsRefundService : PaymentRefundServiceBase
	{
		private readonly ICloudPaymentsApiClient _cloudPaymentsClient;
		private readonly ICloudPaymentsMapper _mapper;

		public CloudPaymentsRefundService(
			ILogger<CloudPaymentsRefundService> logger,
			ICloudPaymentsApiClient cloudPaymentsClient,
			ICloudPaymentsMapper mapper,
			IRefundOperationRepository refundOperationRepository,
			IRefundRequestValidator refundRequestValidator
			) : base(logger, refundOperationRepository, refundRequestValidator)
		{
			_cloudPaymentsClient = cloudPaymentsClient ?? throw new ArgumentNullException(nameof(cloudPaymentsClient));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		public override bool CanHandle(OnlinePaymentSource paymentSource) => paymentSource is OnlinePaymentSource.FromMobileApp;

		protected override async Task<RefundResultDto> ProcessRefundInternalAsync(RefundRequestDto request, string idempotenceKey, CancellationToken cancellationToken)
		{
			if(!long.TryParse(request.TransactionId, out var transactionId))
			{
				Logger.LogWarning("Неверный формат TransactionId: {TransactionId}", request.TransactionId);
				return RefundResultDto.CreateError($"Неверный формат идентификатора транзакции: {request.TransactionId}");
			}

			var transactionResponse = await _cloudPaymentsClient.GetTransactionAsync(transactionId, cancellationToken);

			if(!transactionResponse.Success)
			{
				return RefundResultDto.CreateError($"Не удалось получить транзакцию: {transactionResponse.Message}");
			}

			var transaction = transactionResponse.Model;

			if(transaction.Refunded is true || transaction.Type is CloudPaymentsOperationType.Refund)
			{
				Logger.LogWarning("Попытка повторного возврата по транзакции {TransactionId}", request.TransactionId);
				return RefundResultDto.CreateError("Возврат уже был выполнен");
			}

			var refundDto = _mapper.MapToRefundRequest(request);
			var refundResponse = await _cloudPaymentsClient.RefundAsync(refundDto, idempotenceKey, cancellationToken);

			if(!refundResponse.Success)
			{
				return RefundResultDto.CreateError($"Ошибка возврата: {refundResponse.Message}");
			}

			return _mapper.MapToRefundResult(refundResponse);
		}
	}
}
