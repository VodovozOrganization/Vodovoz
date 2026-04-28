using CustomerOrdersApi.Library.Services.PaymentRefund;
using CustomerOrdersApi.Library.V4.Dto.Orders.CancelOrder;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;

namespace CustomerOrdersApi.Library.Default.Services.PaymentRefund
{
	public abstract class PaymentRefundServiceBase : IPaymentRefundService
	{
		protected ILogger Logger { get; }
		protected IRefundOperationRepository RefundOperationRepository { get; }

		protected PaymentRefundServiceBase(
			ILogger logger,
			IRefundOperationRepository refundOperationRepository)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			RefundOperationRepository = refundOperationRepository ?? throw new ArgumentNullException(nameof(refundOperationRepository));
		}

		public abstract bool CanHandle(OnlinePaymentSource paymentSource);

		public async Task<RefundResultDto> ProcessRefundAsync(IUnitOfWork uow, RefundRequestDto request, CancellationToken cancellationToken)
		{
			try
			{
				var validationError = ValidateRequest(request);
				if(validationError is not null)
				{
					return validationError;
				}

				var existingSuccess = RefundOperationRepository.GetSuccessfulByTransactionId(
					uow,
					request.TransactionId,
					request.OnlineOrder.OnlinePaymentSource);

				if(existingSuccess is not null)
				{
					Logger.LogWarning("Попытка повторного возврата по успешной транзакции {TransactionId}", request.TransactionId);
					return MapFromExistingOperation(existingSuccess);
				}

				var lastAttempt = RefundOperationRepository.GetLastAttemptByTransactionId(
					uow,
					request.TransactionId,
					request.OnlineOrder.OnlinePaymentSource);

				string idempotenceKey;
				RefundOperation operation;

				if(lastAttempt is not null && !lastAttempt.IsSuccess)
				{
					idempotenceKey = lastAttempt.IdempotenceKey;
					operation = lastAttempt;
					Logger.LogInformation("Повторная попытка возврата с ключом {IdempotenceKey}", idempotenceKey);
				}
				else if(lastAttempt is not null && lastAttempt.IsSuccess)
				{
					Logger.LogWarning("Найдена успешная попытка возврата по транзакции {TransactionId}", request.TransactionId);
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
					Logger.LogInformation("Первая попытка возврата с ключом {IdempotenceKey}", idempotenceKey);
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

				return result;
			}
			catch(Exception ex)
			{
				Logger.LogError(ex, "Ошибка возврата для заказа {ExternalOrderId}", request?.ExternalOrderId);

				return RefundResultDto.CreateError("Техническая ошибка при обработке возврата");
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
					ErrorMessage = "Возврат уже был выполнен ранее"
				};
			}

			return new RefundResultDto
			{
				Success = false,
				ErrorMessage = operation.ErrorMessage ?? "Предыдущая попытка возврата завершилась ошибкой"
			};
		}

		/// <summary>
		/// Генерирует ключ идемпотентности для запроса
		/// </summary>
		protected virtual string GenerateIdempotenceKey(RefundRequestDto request)
		{
			var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
			return $"refund_{request.TransactionId}_{timestamp}";
		}

		/// Переделать на возврат DTO
		/// <summary>
		/// Проверяет обязательные параметры запроса
		/// </summary>
		protected virtual RefundResultDto ValidateRequest(RefundRequestDto request)
		{
			if(request is null)
			{
				Logger.LogWarning("Получен пустой запрос");
				return RefundResultDto.CreateError("Запрос не может быть пустым");
			}

			if(request.OnlineOrder is null)
			{
				Logger.LogWarning("OnlineOrder не может быть null для заказа {ExternalOrderId}", request?.ExternalOrderId);
				return RefundResultDto.CreateError("OnlineOrder не может быть null");
			}

			if(request.OnlineOrder.OnlineOrderPaymentStatus is not OnlineOrderPaymentStatus.Paid)
			{
				Logger.LogWarning("Заказ {ExternalOrderId} не оплачен, возврат не требуется", request.OnlineOrder.ExternalOrderId);
				return RefundResultDto.CreateError("Заказ не оплачен, возврат не требуется");
			}

			if(string.IsNullOrEmpty(request.ExternalOrderId))
			{
				Logger.LogWarning("ExternalOrderId не может быть пустым для заказа {OnlineOrderId}", request?.OnlineOrder?.Id);
				return RefundResultDto.CreateError("ExternalOrderId не может быть пустым");
			}

			if(string.IsNullOrEmpty(request.TransactionId))
			{
				Logger.LogWarning("TransactionId не может быть пустым для заказа {ExternalOrderId}", request?.ExternalOrderId);
				return RefundResultDto.CreateError("TransactionId не может быть пустым");
			}

			if(request.Amount <= 0)
			{
				Logger.LogWarning("Сумма возврата {Amount} должна быть больше 0 для заказа {ExternalOrderId}",
					request?.Amount, request?.ExternalOrderId);
				return RefundResultDto.CreateError("Сумма возврата должна быть больше 0");
			}

			return null;
		}
	}
}
