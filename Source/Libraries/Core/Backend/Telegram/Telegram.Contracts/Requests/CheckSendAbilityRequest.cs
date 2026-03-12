using System;
using System.Text.Json.Serialization;

namespace Telegram.Contracts.Requests
{
	/// <summary>
	/// Запрос проверки возможности отправки авторизационного кода в Телеграм
	/// </summary>
	[Serializable]
	public class CheckSendAbilityRequest
	{
		/// <summary>
		/// The phone number for which you want to check our ability to send a verification message, in the E.164 format.
		/// Required: Yes
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("phone_number")]
		public string PhoneNumber { get; private set; }
		
		public static CheckSendAbilityRequest Create(string phoneNumber) => new CheckSendAbilityRequest
		{
			PhoneNumber = phoneNumber
		};
	}
}
