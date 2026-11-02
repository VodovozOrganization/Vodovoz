namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Состояние записи идемпотентного запроса
	/// </summary>
	public enum IdempotencyRecordState
	{
		/// <summary>
		/// Запрос выполняется (метка «в обработке»)
		/// </summary>
		Processing = 1,

		/// <summary>
		/// Запрос выполнен, ответ сохранён
		/// </summary>
		Completed = 2
	}
}
