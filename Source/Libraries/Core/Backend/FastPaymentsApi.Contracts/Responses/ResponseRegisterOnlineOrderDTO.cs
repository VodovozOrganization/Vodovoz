namespace FastPaymentsApi.Contracts.Responses
{
	public class ResponseRegisterOnlineOrderDTO : IErrorResponse
	{
		public string PayUrl { get; set; }
		public string ErrorMessage { get; set; }
	}
}
