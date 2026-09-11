using System.ComponentModel.DataAnnotations;

namespace EdoNotifications.Contracts
{
	/// <summary>
	/// Типы ЭДО уведомления
	/// </summary>
	public enum EdoNotificationType
	{
		/// <summary>
		/// Дубликат кода
		/// </summary>
		[Display(Name = "Дубликат кода")]
		CodeDuplicated = 0,

		/// <summary>
		/// Невалидный контакт для отправки чека
		/// </summary>
		[Display(Name = "Невалидный контакт для отправки чека")]
		ReceiptContactInvalid = 1,

		/// <summary>
		/// Не оплачен заказ самовывоза
		/// </summary>
		[Display(Name = "Не оплачен заказ самовывоза")]
		OrderSelfDeliveryPaymentProblem = 2,

		/// <summary>
		/// Некорректный статус заказа
		/// </summary>
		[Display(Name = "Некорректный статус заказа")]
		OrderStatusProblem = 3,

		/// <summary>
		/// Ошибка наличия кода в пуле
		/// </summary>
		[Display(Name = "Ошибка наличия кода в пуле")]
		CodePoolMissingProblem = 4,

		/// <summary>
		/// Ошибка отправки чека в кассу
		/// </summary>
		[Display(Name = "Ошибка отправки чека в кассу")]
		ReceiptSendingFailed = 5,

		/// <summary>
		/// Ошибка отправки в Такском
		/// </summary>
		[Display(Name = "Ошибка отправки в Такском")]
		TaxcomSendProblem = 6,
	}
}
