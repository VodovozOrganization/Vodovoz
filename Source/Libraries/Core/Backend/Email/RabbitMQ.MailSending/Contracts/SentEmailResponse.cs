namespace RabbitMQ.EmailSending.Contracts
{
	public class SentEmailResponse
	{
		/// <summary>
		/// Конструктор, нужен для десериализации из Json
		/// </summary>
		public SentEmailResponse() { }
		
		private SentEmailResponse(bool sent)
		{
			Sent = sent;
		}
		
		public bool Sent { get; set; }

		public static SentEmailResponse Create(bool sent) => new SentEmailResponse(sent);
	}
}
