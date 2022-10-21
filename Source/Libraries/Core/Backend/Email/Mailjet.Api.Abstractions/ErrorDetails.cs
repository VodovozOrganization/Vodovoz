namespace Mailjet.Api.Abstractions
{
	public class ErrorDetails
	{
		public string ErrorIdentifier { get; set; }
		public string ErrorCode { get; set; }
		public int StatusCode { get; set; }
		public string ErrorMessage { get; set; }
		public string[] ErrorRelatedTo { get; set; }
	}
}
