namespace Vodovoz.Presentation.WebApi
{
	public static class FeatureFlags
	{
		public const string ServiceApiKeyController = "ServiceApiKeyController";

		/// <summary>
		/// Идемпотентность запросов методов, отмеченных атрибутом Idempotent
		/// </summary>
		public const string Idempotency = "Idempotency";

		/// <summary>
		/// Время действия в мобильном приложении берётся из заголовка X-Action-Time-Utc
		/// Выключен - используется время из тела запроса, заголовок не читается
		/// </summary>
		public const string ActionTimeUtcFromHeader = "ActionTimeUtcFromHeader";
	}
}
