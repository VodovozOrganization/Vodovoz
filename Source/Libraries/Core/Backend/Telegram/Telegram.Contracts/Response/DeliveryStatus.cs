using System;
using System.Text.Json.Serialization;

namespace Telegram.Contracts.Response
{
	/// <summary>
	/// This object represents the delivery status of a message
	/// </summary>
	[Serializable]
	public class DeliveryStatus
	{
		/// <summary>
		/// The current status of the message <see cref="DeliveryStatusType"/>
		/// </summary>
		[JsonPropertyName("status")]
		public DeliveryStatusType Status { get; set; }
		/// <summary>
		/// The timestamp when the status was last updated
		/// </summary>
		[JsonPropertyName("updated_at")]
		public long UpdatedAt { get; set; }
	}
}
