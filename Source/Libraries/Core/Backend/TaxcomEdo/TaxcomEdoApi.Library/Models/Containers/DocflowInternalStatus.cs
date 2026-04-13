namespace TaxcomEdoApi.Library.Models.Containers
{
	public enum DocflowInternalStatus
	{
		None,
		OnNegotiation,
		Negotiated,
		FailNegotiation,
		OnSign,
		SignedAndSent,
		FailSign,
		Unknown,
	}
}
