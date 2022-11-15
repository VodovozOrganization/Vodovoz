using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace EdoService
{
	/// <summary>
	/// Ответ на запрос GET /api/v3/auth/cert/key для авторизации.
	/// </summary>
	public class RandomDataResponse
	{
		/// <summary>
		/// уникальный идентификатор сгенерированных случайных данных, тип string
		/// </summary>
		[JsonPropertyName("uuid")]
		public string UUID { get; set; }

		/// <summary>
		/// случайная строка, тип string
		/// </summary>
		/// 
		[JsonPropertyName ("data")]
		public string Data { get; set; }
	}

	/// <summary>
	/// Данные для запроса авторизационного токена.
	/// POST /api/v3/auth/cert/
	/// </summary>
	public class TokenRequest
	{
		/// <summary>
		/// uuid - уникальный идентификатор подписанных данных из /api/v3/auth/cert/key
		/// </summary>
		[JsonPropertyName("uuid")]
		public string UUID { get; set; }

		/// <summary>
		/// подписанные УКЭП зарегистрированного участника случайные данные в base64
		/// </summary>
		[JsonPropertyName("data")]
		public string Data { get; set; }
	}

	/// <summary>
	/// Ответ на запрос авторизационного токена.
	/// POST /api/v3/auth/cert/
	/// </summary>
	public class TokenResponse
	{
		/// <summary>
		/// Авторизационный токен в base64-строке
		/// </summary>
		[JsonPropertyName("token")]
		public string Token { get; set; }
		//[JsonPropertyName("mchdUser")]
		//public string MchdUser { get; set; }
		[JsonPropertyName("code")]
		public string Code { get; set; }
		[JsonPropertyName("error_message")]
		public string ErrorMessage { get; set; }
		[JsonPropertyName("description")]
		public string Description { get; set; }
	}

	public class ErrorDto
	{

	}
}
