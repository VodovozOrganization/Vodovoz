using System;
using System.Net;

namespace TrueMark.Library
{
	/// <summary>
	/// Исключение при ошибке API проверки разрешительного режима (тег 1260)
	/// </summary>
	public class Tag1260ApiException : Exception
	{
		/// <summary>
		/// HTTP-код ответа сервера
		/// </summary>
		public HttpStatusCode StatusCode { get; }

		/// <summary>
		/// Признак временной/инфраструктурной ошибки (408 Request Timeout, 429 Too Many Requests или 5xx Server Error кроме 501 Not Implemented)
		/// </summary>
		public bool IsTransient =>
			(int)StatusCode == 408
			|| (int)StatusCode == 429
			|| ((int)StatusCode >= 500 && StatusCode != HttpStatusCode.NotImplemented);

		public Tag1260ApiException(HttpStatusCode statusCode, string message) : base(message)
		{
			StatusCode = statusCode;
		}

		public Tag1260ApiException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
		{
			StatusCode = statusCode;
		}
	}
}
