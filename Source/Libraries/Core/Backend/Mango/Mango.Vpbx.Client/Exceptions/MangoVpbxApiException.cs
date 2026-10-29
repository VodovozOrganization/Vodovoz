using Mango.Core.Dto.Vpbx.Responses;
using System;
using System.Net;

namespace Mango.Vpbx.Client.Exceptions
{
	/// <summary>
	/// Ошибка выполнения запроса к API ВАТС Манго.
	/// Бросается при неуспешном HTTP-статусе, при коде результата, отличном от 1000,
	/// и при превышении лимита количества запросов
	/// </summary>
	public class MangoVpbxApiException : Exception
	{
		public MangoVpbxApiException(
			string message,
			string endpoint,
			HttpStatusCode statusCode,
			int? resultCode,
			string responseBody)
			: base(message)
		{
			Endpoint = endpoint;
			StatusCode = statusCode;
			ResultCode = resultCode;
			ResponseBody = responseBody;
		}

		/// <summary>
		/// Метод API ВАТС, при вызове которого произошла ошибка
		/// </summary>
		public string Endpoint { get; }

		/// <summary>
		/// HTTP-статус ответа
		/// </summary>
		public HttpStatusCode StatusCode { get; }

		/// <summary>
		/// Код результата API ВАТС, если его удалось получить из тела ответа
		/// </summary>
		public int? ResultCode { get; }

		/// <summary>
		/// Тело ответа
		/// </summary>
		public string ResponseBody { get; }

		/// <summary>
		/// Признак того, что запрос отклонён из-за превышения лимита количества запросов.
		/// Такой запрос можно повторить, сделав паузу или снизив интенсивность обращений к API
		/// </summary>
		public bool IsRateLimitExceeded =>
			(int)StatusCode == 429 || ResultCode == VpbxResultCodes.RateLimitExceeded;
	}
}
