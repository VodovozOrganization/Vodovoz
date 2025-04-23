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
		Accepted,
		/// <summary>
		/// Не принято ЧЗ
		/// </summary>
		Rejected,
		/// <summary>
		/// Успешное аннулирование в ЧЗ
		/// </summary>
		CancellationAccepted,
		/// <summary>
		/// Отмена аннулирования в ЧЗ(ошибка, возможно первично документ не регистрировался)
		/// </summary>
		CancellationRejected
	}
}
