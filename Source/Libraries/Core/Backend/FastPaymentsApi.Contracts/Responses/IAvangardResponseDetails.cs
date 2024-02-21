namespace FastPaymentsApi.Contracts.Responses
{
	public interface IAvangardResponseDetails
	{
		int ResponseCode { get; set; }
		string ResponseMessage { get; set; }
	}
}
