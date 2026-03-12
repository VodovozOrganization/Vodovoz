using System;
using System.Text.Json.Serialization;

namespace Telegram.Contracts.Response
{
	[Serializable]
	public class RequestStatus
	{
		/// <summary>
		/// Unique identifier of the verification request.
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("request_id")]
		public string RequestId { get; set; }
		/// <summary>
		/// The phone number to which the verification code was sent, in the E.164 format.
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("phone_number")]
		public string PhoneNumber { get; set; }
		/// <summary>
		/// Total request cost incurred by either checkSendAbility or sendVerificationMessage.
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("request_cost")]
		public decimal RequestCost { get; set; }
		/// <summary>
		/// 
		/// </summary>
		/// <returns>If True, the request fee was refunded.</returns>
		[JsonPropertyName("is_refunded")]
		public bool IsRefunded { get; set; }
		/// <summary>
		/// Remaining balance in credits. Returned only in response to a request that incurs a charge.
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("remaining_balance")]
		public decimal RemainingBalance { get; set; }
		/// <summary>
		/// The current message delivery status. Returned only if a verification message was sent to the user.
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("delivery_status")]
		public DeliveryStatus DeliveryStatus { get; set; }
		/// <summary>
		/// The current status of the verification process.
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("verification_status")]
		public VerificationStatus VerificationStatus { get; set; }
		/// <summary>
		/// Custom payload if it was provided in the request, 0-256 bytes.
		/// Required: Optional
		/// </summary>
		/// <returns></returns>
		[JsonPropertyName("payload")]
		public string Payload { get; set; }
	}
}
