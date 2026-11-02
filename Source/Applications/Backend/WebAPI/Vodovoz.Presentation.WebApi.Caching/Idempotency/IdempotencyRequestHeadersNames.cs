namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Названия заголовков запроса, относящихся к идемпотентности
	/// </summary>
	public static class IdempotencyRequestHeadersNames
	{
		/// <summary>
		/// Ключ идемпотентности (Guid операции)
		/// </summary>
		public const string IdempotencyKey = "X-Idempotency-Key";

		/// <summary>
		/// Время действия в мобильном приложении, UTC
		/// </summary>
		public const string ActionTimeUtc = "X-Action-Time-Utc";
	}
}
