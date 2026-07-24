namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Коды результатов API ВАТС, возвращаемые методами работы с сотрудниками и группами.
	/// Полный список кодов приведён в документации API ВАТС в разделе "Список кодов результатов"
	/// </summary>
	public static class VpbxResultCodes
	{
		/// <summary>
		/// Действие выполнено успешно
		/// </summary>
		public const int Success = 1000;

		/// <summary>
		/// Переданы неверные параметры команды
		/// </summary>
		public const int InvalidParameters = 3100;

		/// <summary>
		/// Значение ключа не соответствует рассчитанному, то есть не совпала подпись запроса
		/// </summary>
		public const int InvalidSign = 3102;

		/// <summary>
		/// В запросе отсутствует обязательный параметр
		/// </summary>
		public const int RequiredParameterMissing = 3103;

		/// <summary>
		/// Параметр передан в неправильном формате
		/// </summary>
		public const int InvalidParameterFormat = 3104;

		/// <summary>
		/// Неверный ключ доступа
		/// </summary>
		public const int InvalidApiKey = 3105;

		/// <summary>
		/// Данное значение уже используется.
		/// Ожидаемый код при попытке создать сотрудника с уже занятым внутренним номером
		/// </summary>
		public const int ValueAlreadyUsed = 3106;

		/// <summary>
		/// Объект не существует
		/// </summary>
		public const int ObjectNotFound = 3300;

		/// <summary>
		/// Некорректный JSON передан в API ВАТС
		/// </summary>
		public const int InvalidJson = 3400;

		/// <summary>
		/// Превышен лимит количества запросов.
		/// Код внутренний, наружу отдаётся ответом с HTTP-статусом 429
		/// </summary>
		public const int RateLimitExceeded = 4290;

		/// <summary>
		/// Ошибка сервера
		/// </summary>
		public const int ServerError = 5000;

		/// <summary>
		/// Услуга недоступна
		/// </summary>
		public const int ServiceUnavailable = 5008;

		/// <summary>
		/// Запрещено создание/удаление сотрудника-робота
		/// </summary>
		public const int RobotMemberModificationForbidden = 5302;
	}
}
