using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Статус согласия на получение документов по ЭДО
	/// </summary>
	public enum ConsentForEdoStatus
	{
		/// <summary>
		/// Неизвестно
		/// </summary>
		[Display(Name = "Неизвестно")]
		Unknown,
		/// <summary>
		/// Отправлено
		/// </summary>
		[Display(Name = "Отправлено")]
		Sent,
		/// <summary>
		/// Согласен
		/// </summary>
		[Display(Name = "Согласен")]
		Agree,
		/// <summary>
		/// Отклонено
		/// </summary>
		[Display(Name = "Отклонено")]
		Rejected
	}
}
