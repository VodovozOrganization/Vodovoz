namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Названия заголовков ответа, относящихся к идемпотентности
	/// </summary>
	public static class IdempotencyResponseHeadersNames
	{
		/// <summary>
		/// Признак того, что ответ отдан из сохранённого результата первого запроса с тем же ключом,
		/// а метод повторно не выполнялся
		/// </summary>
		public const string IdempotentReplayed = "X-Idempotent-Replayed";
	}
}
