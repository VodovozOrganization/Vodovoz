using System;
using System.Text.Json.Serialization;

namespace Telegram.Contracts.Response
{
	/// <summary>
	/// This object represents the verification status of a code
	/// </summary>
	[Serializable]
	public class VerificationStatus
	{
		/// <summary>
		/// The current status of the verification process <see cref="VerificationStatusType"/>
		/// </summary>
		[JsonPropertyName("status")]
		public VerificationStatusType Status { get; set; }
		//public string Status { get; set; }
		/// <summary>
		/// The timestamp for this particular status. Represents the time when the status was last updated
		/// </summary>
		[JsonPropertyName("updated_at")]
		public long UpdatedAt { get; set; }
		/// <summary>
		/// Optional. The code entered by the user
		/// </summary>
		[JsonPropertyName("code_entered")]
		public string CodeEntered { get; set; }
	}
}
