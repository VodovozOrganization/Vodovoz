using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TrueApi.Dto.Participants
{
	public class ParticipantRegistrationDto
	{
		[JsonPropertyName("inn")]
		public string Inn { get; set; }

		[JsonPropertyName("is_registered")]
		public bool IsRegistered { get; set; }

		[JsonPropertyName("productGroups")]
		public IEnumerable<string> ProductGroups { get; set; }

		[JsonIgnore] 
		public bool IsRegisteredForWater => ProductGroups != null && ProductGroups.Any(pg => pg == "water") && IsRegistered;
	}
}
