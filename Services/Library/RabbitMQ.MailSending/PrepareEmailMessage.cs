namespace RabbitMQ.MailSending
{
	public class PrepareEmailMessage
	{
		public int StoredEmailId { get; set; }
		public int SendAttemptsCount { get; set; }
	}
}
