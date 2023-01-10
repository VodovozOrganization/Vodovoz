namespace Sms.External.Interface
{
	public class SmsSendResult : ISmsSendResult
	{
		public SmsSentStatus Status { get; set; }

		public string ServerId { get; set; }

		public string LocalId { get; set; }

		public string Description { get; set; }

		public SmsSendResult(SmsSentStatus status)
		{
			Status = status;
		}
	}
}
