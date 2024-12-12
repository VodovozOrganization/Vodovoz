using QS.DomainModel.Entity;
using System;

namespace Vodovoz.Core.Domain.Pacs
{
	public class OperatorState : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual int OperatorId { get; set; }
		public virtual OperatorSession Session { get; set; }
		public virtual OperatorWorkshift WorkShift { get; set; }
		public virtual DateTime Started { get; set; }
		public virtual DateTime? Ended { get; set; }
		public virtual OperatorTrigger Trigger { get; set; }
		public virtual OperatorStateType State { get; set; }
		public virtual OperatorBreakType? BreakType { get; set; }
		public virtual string PhoneNumber { get; set; }
		public virtual string CallId { get; set; }
		public virtual DisconnectionType DisconnectionType { get; set; }
		public virtual BreakChangedBy BreakChangedBy { get; set; }
		public virtual int? BreakChangedByAdminId { get; set; }
		public virtual string BreakAdminReason { get; set; }

		public static OperatorState Copy(OperatorState operatorState)
		{
			return new OperatorState
			{
				OperatorId = operatorState.OperatorId,
				Session = operatorState.Session,
				Started = operatorState.Started,
				Ended = operatorState.Ended,
				Trigger = operatorState.Trigger,
				State = operatorState.State,
				PhoneNumber = operatorState.PhoneNumber,
				DisconnectionType = operatorState.DisconnectionType,
				WorkShift = operatorState.WorkShift
			};
		}
	}
}
