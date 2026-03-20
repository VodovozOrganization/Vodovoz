namespace CustomerOrdersApi.Library.Services
{
	public class OperationValidationResult
	{
		/// <summary>
		/// Определяет, может ли операция быть выполнена.
		/// </summary>
		/// <value>
		/// <c>true</c> - операция валидна и может быть выполнена;
		/// <c>false</c> - операция невалидна, причина указана в <see cref="ErrorMessage"/>.
		/// </value>
		public bool IsValid { get; private set; }

		/// <summary>
		/// Сообщение ошибки
		/// </summary>
		public string ErrorMessage { get; private set; }

		/// <summary>
		/// Создает результат валидации
		/// </summary>
		/// <returns>Результат валидации</returns>
		public static OperationValidationResult Valid() =>
			new()
			{ IsValid = true };

		/// <summary>
		/// Создает результат валидации с ошибкой
		/// </summary>
		/// <param name="errorMessage">сообщение с ошибкой</param>
		/// <returns>Результат валидации</returns>
		public static OperationValidationResult Invalid(string errorMessage) =>
			new()
			{ IsValid = false, ErrorMessage = errorMessage };
	}
}
