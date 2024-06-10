namespace FastPaymentsApi.Contracts.Requests
{
	public enum RequestPaymentStatus
	{
		NotFound = 0,
		Processing,
		Rejected,
		Performed
	}
}
