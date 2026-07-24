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
		CodeDuplicatedException
	}
}
