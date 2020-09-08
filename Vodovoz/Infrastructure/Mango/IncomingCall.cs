using System;
using System.Linq;

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

		public string CallerName => message.CallFrom?.Names != null ? String.Join("\n", message.CallFrom.Names.Select(x => x.Name)) : null;
		public string CallerNumber => message.CallFrom?.Number;
	}
}
