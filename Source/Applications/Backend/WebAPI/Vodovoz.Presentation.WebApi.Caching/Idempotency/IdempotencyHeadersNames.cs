namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Названия заголовков идемпотентности
	/// </summary>
	public static class IdempotencyHeadersNames
	{
		/// <summary>
		/// Ключ идемпотентности
		/// </summary>
		public static string IdempotencyKey => "X-Idempotency-Key";

		/// <summary>
		/// Время действия
		/// </summary>
		public static string ActionTimeUtc => "X-Action-Time-Utc";
	}
}
