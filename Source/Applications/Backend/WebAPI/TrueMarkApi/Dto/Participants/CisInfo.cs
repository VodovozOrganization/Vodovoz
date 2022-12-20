using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TrueMarkApi.Dto.Participants
{
	public class CisInfo
	{
		[JsonPropertyName("requestedCis")]
		public string RequestedCis { get; set; }

		[JsonPropertyName("packageType")]
		public bool PackageType { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }
	}
}
