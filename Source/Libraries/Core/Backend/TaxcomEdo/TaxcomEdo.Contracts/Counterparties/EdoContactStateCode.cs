using System.Text.Json.Serialization;

namespace TaxcomEdo.Contracts.Counterparties
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum EdoContactStateCode
	{
		Incoming,
		Sent,
		Accepted,
		Rejected,
		Error
	}
}
