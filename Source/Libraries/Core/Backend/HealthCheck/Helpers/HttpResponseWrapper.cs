using System.Net;

namespace VodovozHealthCheck.Helpers
{
	/// <summary>
	///		Обёртка результата HTTP-вызова с удобными полями для передачи данных, кода статуса и ошибок.
	/// </summary>
	/// <typeparam name="T">Тип полезной нагрузки ответа.</typeparam>
	public class HttpResponseWrapper<T>
	{
		/// <summary>
		///		Данные ответа, десериализованные в тип <typeparamref name="T"/>.
		/// </summary>
		public T Data { get; set; }

		/// <summary>
		///		HTTP статус-код ответа.
		/// </summary>
		public HttpStatusCode StatusCode { get; set; }

		/// <summary>
		///		Признак успешности запроса. true — успешный ответ (обычно 2xx), false — ошибка.
		/// </summary>
		public bool IsSuccess { get; set; }

		/// <summary>
		///		Сообщение об ошибке в случае неуспешного ответа. Может быть null или пустым при успешном запросе.
		/// </summary>
		public string ErrorMessage { get; set; }
	}
}
