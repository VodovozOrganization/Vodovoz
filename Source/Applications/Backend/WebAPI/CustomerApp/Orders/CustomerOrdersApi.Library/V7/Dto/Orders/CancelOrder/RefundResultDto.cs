using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.CancelOrder
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
		/// Текст ошибки (для неуспешных операций)
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Новый статус оплаты онлайн заказа
		/// </summary>
		public OnlineOrderPaymentStatus NewPaymentStatus { get; set; }

		/// <summary>
		/// Создает результат для успешного возврата
		/// </summary>
		public static RefundResultDto CreateSuccess() => new()
		{
			Success = true
		};

		/// <summary>
		/// Создает результат для ошибки возврата
		/// </summary>
		public static RefundResultDto CreateError(string errorMessage) => new()
		{
			Success = false,
			ErrorMessage = errorMessage
		};
	}
}
