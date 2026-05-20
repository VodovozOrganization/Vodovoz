using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Типы событий рассылки
	/// </summary>
	public enum BulkEmailEventType
	{
		/// <summary>
		/// Подписка
		/// </summary>
		[Display(Name = "Подписка")]
		Subscribing,

		/// <summary>
		/// Отписка
		/// </summary>
		[Display(Name = "Отписка")]
		Unsubscribing
	}
}
