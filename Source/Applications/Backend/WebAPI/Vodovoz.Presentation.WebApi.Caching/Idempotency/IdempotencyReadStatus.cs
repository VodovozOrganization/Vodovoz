namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Результат чтения записи идемпотентного запроса
	/// </summary>
	public enum IdempotencyReadStatus
	{
		/// <summary>
		/// Запись найдена и прочитана
		/// </summary>
		Found = 1,

		/// <summary>
		/// Записи нет
		/// </summary>
		NotFound = 2,

		/// <summary>
		/// Хранилище недоступно или запись нечитаема - о наличии записи ничего не известно
		/// </summary>
		Unavailable = 3
	}
}
