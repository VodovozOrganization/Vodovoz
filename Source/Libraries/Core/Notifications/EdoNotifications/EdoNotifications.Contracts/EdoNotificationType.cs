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
		CodeDuplicated,

		/// <summary>
		/// Невалидный контакт для отправки чека
		/// </summary>
		[Display(Name = "Невалидный контакт для отправки чека")]
		ReceiptContactInvalid,

		/// <summary>
		/// Ошибка наличия кода в пуле
		/// </summary>
		[Display(Name = "Ошибка наличия кода в пуле")]
		CodePoolMissingProblem,
	}
}
