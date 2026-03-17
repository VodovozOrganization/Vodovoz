using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.Dto.Orders.CancelOrder
{
	/// <summary>
	/// Результат операции возврата средств
	/// </summary>
	public class RefundResultDto
	{
		public RefundResultDto()
		{
		}

		public RefundResultDto(
			bool success,
			string refundId,
			string errorMessage,
			string cancellationParty,
			string cancellationReason,
			OnlineOrderPaymentStatus newPaymentStatus)
		{
			Success = success;
			RefundId = refundId;
			ErrorMessage = errorMessage;
			CancellationParty = cancellationParty;
			CancellationReason = cancellationReason;
			NewPaymentStatus = newPaymentStatus;
		}
		/// <summary>
		/// Успешность операции
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// ID возврата в платежной системе (для успешных операций)
		/// </summary>
		public string RefundId { get; set; }

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
		/// Новый статус оплаты онлайн заказа
		/// </summary>
		public OnlineOrderPaymentStatus NewPaymentStatus { get; set; }

	}
}
