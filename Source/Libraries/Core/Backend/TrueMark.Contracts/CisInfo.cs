using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	public class CisInfo
	{
		[JsonPropertyName("requestedCis")]
		public string RequestedCis { get; set; }

		[JsonPropertyName("packageType")]
		public string PackageType { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("ownerInn")]
		public string OwnerInn { get; set; }

		[JsonPropertyName("ownerName")]
		public string OwnerName { get; set; }
	}

	public class CisInfoRoot
	{
		[JsonPropertyName("cisInfo")]
		public CisInfo CisInfo { get; set; }
	}
}
