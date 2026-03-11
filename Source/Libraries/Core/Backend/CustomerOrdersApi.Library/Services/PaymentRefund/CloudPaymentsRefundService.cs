using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund.HttpClients;
using CustomerOrdersApi.Library.Services.PaymentRefund.Mappers;
using CustomerOrdersApi.Library.Services.PaymentRefund.Models.CloudPayments;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	public class CloudPaymentsRefundService : PaymentRefundServiceBase
	{
		private readonly ICloudPaymentsHttpClient _cloudPaymentsClient;
		private readonly ICloudPaymentsMapper _mapper;

		public CloudPaymentsRefundService(
			ILogger<CloudPaymentsRefundService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICloudPaymentsHttpClient cloudPaymentsClient,
			ICloudPaymentsMapper mapper,
			IHttpClientFactory httpClientFactory
			) : base(logger, unitOfWorkFactory, httpClientFactory)
		{
			_cloudPaymentsClient = cloudPaymentsClient ?? throw new ArgumentNullException(nameof(cloudPaymentsClient));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		public override bool CanHandle(OnlinePaymentSource paymentSource) => paymentSource is OnlinePaymentSource.FromMobileApp;

		public override async Task<RefundResultDto> ProcessRefundAsync(RefundRequestDto request, CancellationToken cancellationToken)
		{
			try
			{
				ValidateRequest(request);

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

				if(transaction.Refunded is true || transaction.Type is CloudPaymentsOperationType.Refund )
				{
					_logger.LogWarning("Попытка повторного возврата по транзакции {TransactionId}", request.TransactionId);
					return CreateErrorResult("Возврат уже был выполнен");
				}

				var refundDto = _mapper.MapToRefundRequest(request);
				var refundResponse = await _cloudPaymentsClient.RefundAsync(refundDto, cancellationToken);

				if(!refundResponse.Success)
				{
					return CreateErrorResult($"Ошибка возврата: {refundResponse.Message}");
				}

				return _mapper.MapToRefundResult(refundResponse);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка возврата");
				return CreateErrorResult("Техническая ошибка");
			}
		}
	}
}
