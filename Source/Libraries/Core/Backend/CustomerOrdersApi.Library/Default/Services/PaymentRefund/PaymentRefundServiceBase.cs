using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund;
using CustomerOrdersApi.Library.V4.Dto.Orders.CancelOrder;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;

namespace CustomerOrdersApi.Library.Default.Services.PaymentRefund
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
				var validationError = ValidateRequest(request);
				if(validationError is not null)
				{
					return validationError;
				}

				var existingSuccess = _refundOperationRepository.GetSuccessfulByTransactionId(
					uow,
					request.TransactionId,
					request.OnlineOrder.OnlinePaymentSource);

				if(existingSuccess is not null)
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

				if(lastAttempt is not null && !lastAttempt.IsSuccess)
				{
					idempotenceKey = lastAttempt.IdempotenceKey;
					operation = lastAttempt;
					_logger.LogInformation("Повторная попытка возврата с ключом {IdempotenceKey}", idempotenceKey);
				}
				else if(lastAttempt is not null && lastAttempt.IsSuccess)
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

				return result;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка возврата для заказа {ExternalOrderId}", request?.ExternalOrderId);

				return CreateErrorResult("Техническая ошибка при обработке возврата");
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
					NewPaymentStatus = OnlineOrderPaymentStatus.Refund
				};
			}

			return new RefundResultDto
			{
				Success = false,
				ErrorMessage = operation.ErrorMessage ?? "Предыдущая попытка возврата завершилась ошибкой"
			};
		}

		/// <summary>
		/// Создает результат для успешного возврата
		/// </summary>
		protected virtual RefundResultDto CreateSuccessResult(string refundId)
		{
			return new RefundResultDto(true, null, default, OnlineOrderPaymentStatus.Refund);
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
			string errorMessage)
		{
			return new RefundResultDto(false, default, errorMessage, OnlineOrderPaymentStatus.Paid);
		}

		/// Переделать на возврат DTO
		/// <summary>
		/// Проверяет обязательные параметры запроса
		/// </summary>
		protected virtual RefundResultDto ValidateRequest(RefundRequestDto request)
		{
			if(request == null)
			{
				_logger.LogWarning("Получен пустой запрос");
				return CreateErrorResult("Запрос не может быть пустым");
			}

			if(request.OnlineOrder is null)
			{
				_logger.LogWarning("OnlineOrder не может быть null для заказа {ExternalOrderId}", request?.ExternalOrderId);
				return CreateErrorResult("OnlineOrder не может быть null");
			}

			if(request.OnlineOrder.OnlineOrderPaymentStatus is not OnlineOrderPaymentStatus.Paid)
			{
				_logger.LogWarning("Заказ {ExternalOrderId} не оплачен, возврат не требуется", request.OnlineOrder.ExternalOrderId);
				return CreateErrorResult("Заказ не оплачен, возврат не требуется");
			}

			if(string.IsNullOrEmpty(request.ExternalOrderId))
			{
				_logger.LogWarning("ExternalOrderId не может быть пустым для заказа {OnlineOrderId}", request?.OnlineOrder?.Id);
				return CreateErrorResult("ExternalOrderId не может быть пустым");
			}

			if(string.IsNullOrEmpty(request.TransactionId))
			{
				_logger.LogWarning("TransactionId не может быть пустым для заказа {ExternalOrderId}", request?.ExternalOrderId);
				return CreateErrorResult("TransactionId не может быть пустым");
			}

			if(request.Amount <= 0)
			{
				_logger.LogWarning("Сумма возврата {Amount} должна быть больше 0 для заказа {ExternalOrderId}",
					request?.Amount, request?.ExternalOrderId);
				return CreateErrorResult("Сумма возврата должна быть больше 0");
			}

			return null;
		}
	}
}
