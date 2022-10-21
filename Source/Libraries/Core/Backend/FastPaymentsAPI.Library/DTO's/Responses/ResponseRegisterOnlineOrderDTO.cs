namespace FastPaymentsAPI.Library.DTO_s.Responses
{
	public class ResponseRegisterOnlineOrderDTO : IErrorResponse
	{
		public string PayUrl { get; set; }
		public string ErrorMessage { get; set; }
	}
}
