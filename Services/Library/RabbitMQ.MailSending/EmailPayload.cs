namespace RabbitMQ.MailSending
{
	public class EmailPayload
	{
		public int Id { get; set; }
		public bool Trackable { get; set; }
		public int InstanceId { get; set; }
	}
}
