namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
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

		public static YandexPayResult<T> FromSuccess(T data)
		{
			return new YandexPayResult<T>
			{
				Success = true,
				Data = data
			};
		}

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
