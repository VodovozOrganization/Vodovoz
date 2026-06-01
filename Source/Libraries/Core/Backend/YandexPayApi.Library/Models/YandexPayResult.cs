namespace YandexPayApi.Library.Models
{
	/// <summary>
	/// Универсальная модель ответа от Yandex Pay API
	/// </summary>
	/// <typeparam name="T">Тип данных ответа</typeparam>
	public class YandexPayResult<T>
	{
		/// <summary>
		/// Успешность операции
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// Данные ответа (при успехе)
		/// </summary>
		public T Data { get; set; }

		/// <summary>
		/// Сообщение об ошибке (при неудаче)
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Код ошибки (при неудаче)
		/// </summary>
		public string ErrorCode { get; set; }

		/// <summary>
		/// Детали ошибки (при неудаче)
		/// </summary>
		public object ErrorDetails { get; set; }

		/// <summary>
		/// Создает успешный результат
		/// </summary>
		/// <param name="data">Данные ответа</param>
		/// <returns>Успешный результат с данными</returns>
		public static YandexPayResult<T> FromSuccess(T data)
		{
			return new YandexPayResult<T>
			{
				Success = true,
				Data = data
			};
		}

		/// <summary>
		/// Создает результат с ошибкой
		/// </summary>
		/// <param name="errorMessage">Сообщение об ошибке</param>
		/// <param name="errorCode">Код ошибки</param>
		/// <param name="errorDetails">Детали ошибки</param>
		/// <returns>Результат с ошибкой</returns>
		public static YandexPayResult<T> FromError(string errorMessage, string errorCode = null, object errorDetails = null)
		{
			return new YandexPayResult<T>
			{
				Success = false,
				ErrorMessage = errorMessage,
				ErrorCode = errorCode,
				ErrorDetails = errorDetails
			};
		}
	}
}
