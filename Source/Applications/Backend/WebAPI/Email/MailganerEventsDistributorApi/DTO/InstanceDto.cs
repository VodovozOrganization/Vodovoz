namespace MailganerEventsDistributorApi.DTO
{
	public class InstanceDto
	{
		public int Id { get; set; }
		public int DatabaseId { get; set; }
		public string MessageBrockerHost { get; set; }
		public string MessageBrockerVirtualHost { get; set; }
		public int Port { get; set; }
	}
}
