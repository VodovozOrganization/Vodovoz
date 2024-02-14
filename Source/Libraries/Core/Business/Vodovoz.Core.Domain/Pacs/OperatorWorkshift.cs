using System;

namespace Vodovoz.Core.Domain.Pacs
{
	public class OperatorWorkshift
	{
		public virtual int Id { get; set; }
		public virtual int OperatorId { get; set; }
		public virtual WorkShift PlannedWorkShift { get; set; }
		public virtual DateTime Started { get; set; }
		public virtual DateTime? Ended { get; set; }
		public virtual string Reason { get; set; }

		public virtual DateTime GetPlannedEndTime()
		{
			return Started.Add(PlannedWorkShift.Duration);
		}
	}
}
