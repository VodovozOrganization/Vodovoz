namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	/// <summary>
	/// Общий класс результата для API ЮKassa
	/// </summary>
	public class YooKassaResult<T>
	{
		public bool Success { get; set; }
		public T Data { get; set; }
		public string ErrorMessage { get; set; }
		public string ErrorCode { get; set; }
		public string ErrorParameter { get; set; }

		public static YooKassaResult<T> FromSuccess(T data)
		{
			return new YooKassaResult<T>
			{
				Success = true,
				Data = data
			};
		}

		public static YooKassaResult<T> FromError(string errorMessage, string errorCode = null, string errorParameter = null)
		{
			return new YooKassaResult<T>
			{
				Success = false,
				ErrorMessage = errorMessage,
				ErrorCode = errorCode,
				ErrorParameter = errorParameter
			};
		}
	}
}
