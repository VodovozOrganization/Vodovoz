using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Причина
	/// </summary>
	public enum Reason
	{
		/// <summary>
		/// Неизвестна
		/// </summary>
		[Display(Name = "Неизвестна")]
		Unknown,
		/// <summary>
		/// Сервис
		/// </summary>
		[Display(Name = "Сервис")]
		Service,
		/// <summary>
		/// Аренда
		/// </summary>
		[Display(Name = "Аренда")]
		Rent,
		/// <summary>
		/// Расторжение
		/// </summary>
		[Display(Name = "Расторжение")]
		Cancellation,
		/// <summary>
		/// Продажа
		/// </summary>
		[Display(Name = "Продажа")] Sale
	}
}
