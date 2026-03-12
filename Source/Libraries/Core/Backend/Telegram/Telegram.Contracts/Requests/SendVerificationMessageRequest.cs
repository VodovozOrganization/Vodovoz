using System;
using System.Text.Json.Serialization;

namespace Telegram.Contracts.Requests
{
	/// <summary>
	/// Отправка авторизационного кода через Телеграм
	/// </summary>
	[Serializable]
	public class SendVerificationMessageRequest
	{
		private const int _codeLengthMinValue = 4;
		private const int _codeLengthMaxValue = 8;
		private const int _ttlMinValue = 30;
		private const int _ttlMaxValue = 3600;
		
		/// <summary>
		/// The phone number to which you want to send a verification message, in the E.164 format.
		/// Required: Yes
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("phone_number")]
		public string PhoneNumber { get; private set; }
		/// <summary>
		/// The unique identifier of a previous request from checkSendAbility. If provided, this request will be free of charge.
		/// Required: Optional
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("request_id")]
		public string RequestId { get; private set; }
		/// <summary>
		/// Username of the Telegram channel from which the code will be sent.
		/// The specified channel, if any, must be verified and owned by the same account who owns the Gateway API token.
		/// Required: Optional
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("sender_username")]
		public string Sender { get; private set; }
		/// <summary>
		/// The verification code. Use this parameter if you want to set the verification code yourself.
		/// Only fully numeric strings between 4 and 8 characters in length are supported. If this parameter is set, code_length is ignored.
		/// Required: Optional
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("code")]
		public string Code { get; private set; }
		/// <summary>
		/// The length of the verification code if Telegram needs to generate it for you. Supported values are from 4 to 8.
		/// This is only relevant if you are not using the code parameter to set your own code.
		/// Use the checkVerificationStatus method with the code parameter to verify the code entered by the user.
		/// Required: Optional
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("code_length")]
		public int? CodeLength { get; private set; }
		/// <summary>
		/// An HTTPS URL where you want to receive delivery reports related to the sent message, 0-256 bytes.
		/// Required: Optional
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("callback_url")]
		public string CallbackUrl { get; private set; }
		/// <summary>
		/// Custom payload, 0-128 bytes. This will not be displayed to the user, use it for your internal processes.
		/// Required: Optional
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("payload")]
		public string Payload { get; private set; }
		/// <summary>
		/// Time-to-live (in seconds) before the message expires. If the message is not delivered or read within this time,
		/// the request fee will be refunded. Supported values are from 30 to 3600.
		/// Required: Optional
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("ttl")]
		public int? Ttl { get; private set; }

		public static SendVerificationMessageRequest Create(
			string phoneNumber,
			string requestId = null,
			string sender = null,
			string code = null,
			int? codeLength = null,
			string callbackUrl = null,
			string payload = null,
			int? ttl = null
			)
		{
			if(!string.IsNullOrWhiteSpace(code) && codeLength.HasValue)
			{
				throw new InvalidOperationException("При переданном коде нельзя передавать параметр длины кода для генерации");
			}

			if(string.IsNullOrWhiteSpace(code) && !codeLength.HasValue)
			{
				throw new InvalidOperationException("Если не передан код, то должна быть передана длина кода для генерации");
			}
			
			if(codeLength.HasValue && (codeLength.Value < _codeLengthMinValue || codeLength.Value > _codeLengthMaxValue))
			{
				throw new InvalidOperationException(
					$"Длина кода для генерации должна быть в пределах от {_codeLengthMinValue} по {_codeLengthMaxValue} символов");
			}
			
			if(ttl.HasValue && (ttl.Value < _ttlMinValue || ttl.Value > _ttlMaxValue))
			{
				throw new InvalidOperationException(
					$"Срок жизни кода в секундах должен быть в диапазоне от {_ttlMinValue} по {_ttlMaxValue}");
			}
			
			return new SendVerificationMessageRequest
			{
				PhoneNumber = phoneNumber,
				RequestId = requestId,
				Sender = sender,
				Code = code,
				CodeLength = codeLength,
				CallbackUrl = callbackUrl,
				Payload = payload,
				Ttl = ttl
			};
		}
	}
}
