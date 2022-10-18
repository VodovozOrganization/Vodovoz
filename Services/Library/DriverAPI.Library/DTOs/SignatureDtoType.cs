using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum SignatureDtoType
	{
		BySeal,
		ByProxy,
		ProxyOnDeliveryPoint,
		SignatureTranscript
	}
}
