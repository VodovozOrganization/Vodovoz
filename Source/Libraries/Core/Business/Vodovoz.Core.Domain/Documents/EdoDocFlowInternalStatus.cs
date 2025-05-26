using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Внутренний статус документооборота ЭДО Таксоком
	/// </summary>
	public enum EdoDocFlowInternalStatus
	{
		/// <summary>
		/// Нет
		/// </summary>
		[Display(Name = "Нет")]
		None,
		/// <summary>
		/// На аннулировании
		/// </summary>
		[Display(Name = "На аннулировании")]
		OnNegotiation,
		/// <summary>
		/// Аннулировано
		/// </summary>
		[Display(Name = "Аннулировано")]
		Negotiated,
		/// <summary>
		/// Провал аннулирования
		/// </summary>
		[Display(Name = "Провал аннулирования")]
		FailNegotiation,
		/// <summary>
		/// На подписании
		/// </summary>
		[Display(Name = "На подписи")]
		OnSign,
		/// <summary>
		/// Подписано и отправлено
		/// </summary>
		[Display(Name = "Подписано и отправлено")]
		SignedAndSent,
		/// <summary>
		/// Ошибка подписи
		/// </summary>
		[Display(Name = "Ошибка подписи")]
		FailSign,
	}
}
