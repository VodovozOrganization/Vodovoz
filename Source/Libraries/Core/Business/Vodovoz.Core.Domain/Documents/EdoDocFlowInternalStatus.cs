using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Внутренний статус документооборота ЭДО Таксоком
	/// </summary>
	public enum EdoDocFlowInternalStatus
	{
		[Display(Name = "Нет")]
		None,
		[Display(Name = "На аннулировании")]
		OnNegotiation,
		[Display(Name = "Аннулировано")]
		Negotiated,
		[Display(Name = "Провал аннулирования")]
		FailNegotiation,
		[Display(Name = "На подписи")]
		OnSign,
		[Display(Name = "Подписано и отправлено")]
		SignedAndSent,
		[Display(Name = "Ошибка подписи")]
		FailSign,
	}
}
