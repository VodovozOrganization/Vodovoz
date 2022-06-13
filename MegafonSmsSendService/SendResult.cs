using SmsSendInterface;

namespace MegafonSmsSendService
{
	public class SendResult : ISmsSendResult
	{
		public SmsSentStatus Status { get; set; }

		public string ServerId { get; set; }

		public string LocalId { get; set; }

		public string Description { get; set; }
	}
}
