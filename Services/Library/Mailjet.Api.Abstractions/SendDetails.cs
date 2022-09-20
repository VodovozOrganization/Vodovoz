namespace Mailjet.Api.Abstractions
{
	public class SendDetails
	{
		public string Email { get; set; }
		public string MessageUUID { get; set; }
		public long MessageID { get; set; }
		public string MessageHref { get; set; }
	}
}
