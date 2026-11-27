using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Статус регистрации в Честном Знаке
	/// </summary>
	public enum RegistrationInChestnyZnakStatus
	{
		/// <summary>
		/// Неизвестно
		/// </summary>
		[Display(Name = "Неизвестно")]
		Unknown,
		/// <summary>
		/// В процессе регистрации
		/// </summary>
		[Display(Name = "В процессе регистрации")]
		InProcess,
		/// <summary>
		/// Зарегистрирован
		/// </summary>
		[Display(Name = "Зарегистрирован")]
		Registered,
		/// <summary>
		/// Заблокирован
		/// </summary>
		[Display(Name = "Заблокирован")]
		Blocked
	}
}
