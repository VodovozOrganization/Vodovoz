namespace RabbitMQ.EmailSending.Contracts
{
	public class SentEmailResponse
	{
		private SentEmailResponse(bool sent)
		{
			Sent = sent;
		}
		
		public bool Sent { get; }

		public static SentEmailResponse Create(bool sent) => new SentEmailResponse(sent);
	}
}
