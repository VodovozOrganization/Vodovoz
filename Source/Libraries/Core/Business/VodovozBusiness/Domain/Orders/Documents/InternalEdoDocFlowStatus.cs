using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders.Documents
{
	/// <summary>
	/// Внутренний статус документооборота на сервере Такском
	/// </summary>
	public enum InternalEdoDocFlowStatus
	{
		/// <summary>
		/// Без статуса
		/// </summary>
		[Display(Name = "Нет статуса")]
		None,
		/// <summary>
		/// Ожидает аннулирования
		/// </summary>
		[Display(Name = "Ожидает аннулирования")]
		OnNegotiation,
		/// <summary>
		/// Аннулирован
		/// </summary>
		[Display(Name = "Аннулирован")]
		Negotiated,
		/// <summary>
		/// Отказ аннулирования
		/// </summary>
		[Display(Name = "Провал аннулирования")]
		FailNegotiation,
		/// <summary>
		/// На подписи
		/// </summary>
		[Display(Name = "На подписи")]
		OnSign,
		/// <summary>
		/// Подписан и отправлен
		/// </summary>
		[Display(Name = "Подписан и отправлен")]
		SignedAndSent,
		/// <summary>
		/// Ошибка подписи
		/// </summary>
		[Display(Name = "Ошибка подписи")]
		FailSign
	}
}
