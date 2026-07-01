namespace CustomerOrdersApi.Library.SiteOrdersImport.Services
{
	/// <summary>
	/// Результат проверки пакета выгрузки с сайта.
	/// Позволяет отделить ошибки структуры запроса от ошибки авторизации.
	/// </summary>
	public class SiteOrdersImportRequestValidationResult
	{
		private SiteOrdersImportRequestValidationResult(bool isValid, bool isUnauthorized, string message)
		{
			IsValid = isValid;
			IsUnauthorized = isUnauthorized;
			Message = message;
		}

		/// <summary>
		/// Пакет прошёл проверку.
		/// </summary>
		public bool IsValid { get; }

		/// <summary>
		/// Ошибка должна быть возвращена как 401 Unauthorized.
		/// </summary>
		public bool IsUnauthorized { get; }

		/// <summary>
		/// Текст ошибки проверки.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Создаёт успешный результат проверки.
		/// </summary>
		public static SiteOrdersImportRequestValidationResult Success()
		{
			return new SiteOrdersImportRequestValidationResult(true, false, null);
		}

		/// <summary>
		/// Создаёт результат ошибки структуры запроса.
		/// </summary>
		public static SiteOrdersImportRequestValidationResult ValidationError(string message)
		{
			return new SiteOrdersImportRequestValidationResult(false, false, message);
		}

		/// <summary>
		/// Создаёт результат ошибки авторизации.
		/// </summary>
		public static SiteOrdersImportRequestValidationResult Unauthorized(string message)
		{
			return new SiteOrdersImportRequestValidationResult(false, true, message);
		}
	}
}
