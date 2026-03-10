using System;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;

namespace CustomerOrdersApi.Library.Dto.Orders.CancelOrder
{
	/// <summary>
	/// Результат операции возврата средств
	/// </summary>
	public class RefundResultDto
	{
		/// <summary>
		/// Успешность операции
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// ID возврата в платежной системе (для успешных операций)
		/// </summary>
		public string RefundId { get; set; }

		/// <summary>
		/// ID операции для асинхронных возвратов (Яндекс Сплит)
		/// </summary>
		public string OperationId { get; set; }

		/// <summary>
		/// Текст ошибки (для неуспешных операций)
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Инициатор отмены (для YooKassa)
		/// </summary>
		public string CancellationParty { get; set; }

		/// <summary>
		/// Причина отмены (для YooKassa)
		/// </summary>
		public string CancellationReason { get; set; }

		/// <summary>
		/// Время обработки
		/// </summary>
		public DateTime ProcessedAt { get; set; }

		/// <summary>
		/// Статус возврата
		/// </summary>
		public RefundStatus RefundStatus { get; set; }

		/// <summary>
		/// Новый статус оплаты онлайн заказа
		/// </summary>
		public OnlineOrderPaymentStatus NewPaymentStatus { get; set; }

		public RefundResultDto()
		{
		}

		public RefundResultDto(
			bool success,
			string refundId,
			string operationId,
			string errorMessage,
			string cancellationParty,
			string cancellationReason,
			DateTime processedAt,
			RefundStatus refundStatus,
			OnlineOrderPaymentStatus newPaymentStatus)
		{
			Success = success;
			RefundId = refundId;
			OperationId = operationId;
			ErrorMessage = errorMessage;
			CancellationParty = cancellationParty;
			CancellationReason = cancellationReason;
			ProcessedAt = processedAt;
			RefundStatus = refundStatus;
			NewPaymentStatus = newPaymentStatus;
		}
	}
}
