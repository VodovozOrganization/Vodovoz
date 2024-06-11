using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	public class ParticipantRegistrationDto
	{
		[JsonPropertyName("inn")]
		public string Inn { get; set; }

		[JsonPropertyName("is_registered")]
		public bool IsRegistered { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("error_message")]
		public string ErrorMessage { get; set; }

		[JsonPropertyName("productGroups")]
		public IEnumerable<string> ProductGroups { get; set; }

		[JsonIgnore]
		public bool IsRegisteredForWater => ProductGroups != null && ProductGroups.Any(pg => pg == "water") && IsRegistered;
	}
}
