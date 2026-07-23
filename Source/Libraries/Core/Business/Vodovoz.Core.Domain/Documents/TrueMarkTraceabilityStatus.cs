using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Статус прослеживаемости в ЧЗ
	/// </summary>
	public enum TrueMarkTraceabilityStatus
	{
		[Display(Name = "Принято ЧЗ")]
		Accepted,

		[Display(Name = "Не принято ЧЗ")]
		Rejected,

		[Display(Name = "Успешно аннулировано ЧЗ")]
		CancellationAccepted,

		[Display(Name = "Отмена аннулирования ЧЗ")]
		CancellationRejected
	}
}
