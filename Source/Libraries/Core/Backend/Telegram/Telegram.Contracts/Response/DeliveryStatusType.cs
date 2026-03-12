using System;
using System.Text.Json.Serialization;

namespace Telegram.Contracts.Response
{
	/// <summary>
	/// Состояние кода авторизации
	/// </summary>
	[Serializable]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum DeliveryStatusType
	{
		/// <summary>
		/// The message has been sent to the recipient's device(s)
		/// </summary>
		[JsonPropertyName("sent")]
		Sent,
		/// <summary>
		/// The message has been delivered to the recipient's device(s)
		/// </summary>
		[JsonPropertyName("delivered")]
		Delivered,
		/// <summary>
		/// The message has been read by the recipient
		/// </summary>
		[JsonPropertyName("read")]
		Read,
		/// <summary>
		/// The message has expired without being delivered or read
		/// </summary>
		[JsonPropertyName("expired")]
		Expired,
		/// <summary>
		/// The message has been revoked
		/// </summary>
		[JsonPropertyName("revoked")]
		Revoked
	}
}
