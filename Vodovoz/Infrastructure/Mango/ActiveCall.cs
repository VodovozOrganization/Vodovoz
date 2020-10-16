using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Infrastructure.Mango
{
	public class ActiveCall
	{
		public readonly NotificationMessage Message;

		public ActiveCall(NotificationMessage message)
		{
			this.Message = message ?? throw new ArgumentNullException(nameof(message));
		}

		public string CallId => Message.CallId;
		public CallState CallState => Message.State;

		public DateTime? StageBegin => Message.Timestamp.ToDateTime();
		public TimeSpan? StageDuration => DateTime.UtcNow - StageBegin;

		public string CallerName => Message.CallFrom?.Names != null ? String.Join("\n", Message.CallFrom.Names.Select(x => x.Name)) : null;
		public string CallerNumber => Message.CallFrom?.Number;

		#region Ids
		public IEnumerable<int> CounterpartyIds => Message.CallFrom.Names?.Where(n => n.CounterpartyId > 0).Select(n => Convert.ToInt32(n.CounterpartyId)).Distinct();
		public int EmployeeId => Message.CallFrom.Names.Where(n => n.EmployeeId > 0).Select(n => Convert.ToInt32(n.EmployeeId)).FirstOrDefault();
		#endregion
		public bool IsOutgoing => Message?.Direction == CallDirection.Outgoing;
	}
}
