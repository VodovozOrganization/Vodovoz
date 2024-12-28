using System.Xml.Serialization;

namespace TaxcomEdo.Api.Dto
{
	[Serializable]
	[XmlType(AnonymousType = true)]
	public enum ContactStateCode
	{
		[XmlEnum("Incoming")]
		Incoming,
		[XmlEnum("Sent")]
		Sent,
		[XmlEnum("Accepted")]
		Accepted,
		[XmlEnum("Rejected")]
		Rejected,
		[XmlEnum("Error")]
		Error
	}
}
