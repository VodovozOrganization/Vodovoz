using System;
namespace Vodovoz.Infrastructure.Mango
{
	public class IncomingCall
	{
		private readonly NotificationMessage message;

		public IncomingCall(NotificationMessage message)
		{
			this.message = message ?? throw new ArgumentNullException(nameof(message));
		}

		public string CallId => message.CallId;

		public DateTime? StageBegin => message.Timestamp.ToDateTime();
		public TimeSpan? StageDuration => DateTime.Now - StageBegin;

		public string CallerName => message.CallFrom.Name;
		public string CallerNumber => message.CallFrom.Number;
	}
}
