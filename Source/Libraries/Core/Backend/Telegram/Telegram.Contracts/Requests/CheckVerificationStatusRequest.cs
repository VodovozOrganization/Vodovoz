using System;
using System.Text.Json.Serialization;

namespace Telegram.Contracts.Requests
{
	/// <summary>
	/// Проверка статуса кода авторизации
	/// </summary>
	[Serializable]
	public class CheckVerificationStatusRequest
	{
		/// <summary>
		/// The unique identifier of the verification request whose status you want to check.
		/// Required: Yes
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("request_id")]
		public string RequestId { get; private set; }
		/// <summary>
		/// The code entered by the user. If provided, the method checks if the code is valid for the relevant request.
		/// Required: Optional
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("code")]
		public string Code { get; private set; }

		public static CheckVerificationStatusRequest Create(string requestId, string code) =>
			new CheckVerificationStatusRequest
			{
				RequestId = requestId,
				Code = code
			};
	}
}
