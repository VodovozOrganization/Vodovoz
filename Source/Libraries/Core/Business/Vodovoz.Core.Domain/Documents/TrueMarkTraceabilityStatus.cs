using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Статус прослеживаемости в ЧЗ
	/// </summary>
	public enum TrueMarkTraceabilityStatus
	{
		/// <summary>
		/// Принято ЧЗ
		/// </summary>
		[Display(Name = "Принято ЧЗ")]
		Accepted,

		/// <summary>
		/// Не принято ЧЗ
		/// </summary>
		[Display(Name = "Не принято ЧЗ")]
		Rejected,

		/// <summary>
		/// Успешное аннулирование в ЧЗ
		/// </summary>
		[Display(Name = "Успешно аннулировано ЧЗ")]
		CancellationAccepted,

		/// <summary>
		/// Отмена аннулирования в ЧЗ(ошибка, возможно первично документ не регистрировался)
		/// </summary>
		[Display(Name = "Отмена аннулирования ЧЗ")]
		CancellationRejected
	}
}
