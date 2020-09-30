using System;
using System.Linq;

namespace Vodovoz.Infrastructure.Mango
{
	public class IncomingCall
	{
		public readonly NotificationMessage Message;

		public IncomingCall(NotificationMessage message)
		{
			this.Message = message ?? throw new ArgumentNullException(nameof(message));
		}

		public string CallId => Message.CallId;

		public DateTime? StageBegin => Message.Timestamp.ToDateTime();
		public TimeSpan? StageDuration => DateTime.UtcNow - StageBegin;

		public string CallerName => Message.CallFrom?.Names != null ? String.Join("\n", Message.CallFrom.Names.Select(x => x.Name)) : null;
		public string CallerNumber => Message.CallFrom?.Number;

		public bool IsOutgoing => Message?.Direction == CallDirection.Outgoing;
	}
}
