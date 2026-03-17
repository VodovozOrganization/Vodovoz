using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;

namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	public abstract class PaymentRefundServiceBase : IPaymentRefundService
	{
		protected readonly ILogger _logger;
		protected readonly IUnitOfWorkFactory _unitOfWorkFactory;
		protected readonly IHttpClientFactory _httpClientFactory;
		protected readonly IRefundOperationRepository _refundOperationRepository;

		protected PaymentRefundServiceBase(
			ILogger logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHttpClientFactory httpClientFactory,
			IRefundOperationRepository refundOperationRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_refundOperationRepository = refundOperationRepository ?? throw new ArgumentNullException(nameof(refundOperationRepository));
		}

		public abstract bool CanHandle(OnlinePaymentSource paymentSource);

		public async Task<RefundResultDto> ProcessRefundAsync(IUnitOfWork uow, RefundRequestDto request, CancellationToken cancellationToken)
		{
			try
			{
				ValidateRequest(request);

				var existingSuccess = _refundOperationRepository.GetSuccessfulByTransactionId(
					uow,
					request.TransactionId,
					request.OnlineOrder.OnlinePaymentSource);

				if(existingSuccess != null)
				{
					_logger.LogWarning("Попытка повторного возврата по успешной транзакции {TransactionId}", request.TransactionId);
					return MapFromExistingOperation(existingSuccess);
				}

				var lastAttempt = _refundOperationRepository.GetLastAttemptByTransactionId(
					uow,
					request.TransactionId,
					request.OnlineOrder.OnlinePaymentSource);

				string idempotenceKey;
				RefundOperation operation;

				if(lastAttempt != null && !lastAttempt.IsSuccess)
				{
					idempotenceKey = lastAttempt.IdempotenceKey;
					operation = lastAttempt;
					_logger.LogInformation("Повторная попытка возврата с ключом {IdempotenceKey}", idempotenceKey);
				}
				else if(lastAttempt != null && lastAttempt.IsSuccess)
				{
					_logger.LogWarning("Найдена успешная попытка возврата по транзакции {TransactionId}", request.TransactionId);
					return MapFromExistingOperation(lastAttempt);
				}
				else
				{
					idempotenceKey = GenerateIdempotenceKey(request);

					operation = RefundOperation.Create(
						key: idempotenceKey,
						onlineOrderId: request.OnlineOrder.Id,
						transactionId: request.TransactionId,
						paymentSource: request.OnlineOrder.OnlinePaymentSource);

					await uow.SaveAsync(operation, cancellationToken: cancellationToken);
					_logger.LogInformation("Первая попытка возврата с ключом {IdempotenceKey}", idempotenceKey);
				}

				var result = await ProcessRefundInternalAsync(request, idempotenceKey, cancellationToken);

				if(result.Success)
				{
					operation.MarkAsSucceeded(result.RefundId);
				}
				else
				{
					operation.MarkAsFailed(result.ErrorMessage);
				}

				await uow.SaveAsync(operation, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				return result;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка возврата для заказа {ExternalOrderId}", request?.ExternalOrderId);

				return CreateErrorResult("Техническая ошибка при обработке возврата");
			}
			finally
			{
				uow?.Dispose();
			}
		}

		/// <summary>
		/// Специфичная для каждой ПС логика возврата
		/// </summary>
		protected abstract Task<RefundResultDto> ProcessRefundInternalAsync(
			RefundRequestDto request,
			string idempotenceKey,
			CancellationToken cancellationToken);

		/// <summary>
		/// Маппинг из существующей операции в результат
		/// </summary>
		private static RefundResultDto MapFromExistingOperation(RefundOperation operation)
		{
			if(operation.IsSuccess)
			{
				return new RefundResultDto
				{
					Success = true,
					RefundId = operation.RefundId,
					ErrorMessage = "Возврат уже был выполнен ранее",
					NewPaymentStatus = OnlineOrderPaymentStatus.Refunded
				};
			}

			return new RefundResultDto
			{
				Success = false,
				ErrorMessage = operation.ErrorMessage ?? "Предыдущая попытка возврата завершилась ошибкой",
				NewPaymentStatus = OnlineOrderPaymentStatus.Paid
			};
		}

		/// <summary>
		/// Создает результат для успешного возврата
		/// </summary>
		protected virtual RefundResultDto CreateSuccessResult(string refundId)
		{
			return new RefundResultDto(true, refundId, default, default, default, OnlineOrderPaymentStatus.Refunded);
		}

		/// <summary>
		/// Генерирует ключ идемпотентности для запроса
		/// </summary>
		protected virtual string GenerateIdempotenceKey(RefundRequestDto request)
		{
			var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
			return $"refund_{request.TransactionId}_{timestamp}";
		}

		/// <summary>
		/// Создает результат для ошибки возврата
		/// </summary>
		protected virtual RefundResultDto CreateErrorResult(
			string errorMessage,
			string cancellationParty = null,
			string cancellationReason = null)
		{
			return new RefundResultDto(false, default, default, errorMessage, cancellationParty, OnlineOrderPaymentStatus.Paid);
		}

		/// <summary>
		/// Проверяет обязательные параметры запроса
		/// </summary>
		protected virtual void ValidateRequest(RefundRequestDto request)
		{
			if(request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if(request.OnlineOrder is null)
			{
				throw new ArgumentException("OnlineOrder не может быть null", nameof(request));
			}

			if(string.IsNullOrEmpty(request.ExternalOrderId))
			{
				throw new ArgumentException("ExternalOrderId не может быть пустым", nameof(request));
			}

			if(string.IsNullOrEmpty(request.TransactionId))
			{
				throw new ArgumentException("TransactionId не может быть пустым", nameof(request));
			}

			if(request.Amount <= 0)
			{
				throw new ArgumentException("Сумма возврата должна быть больше 0", nameof(request));
			}
		}
	}
}
