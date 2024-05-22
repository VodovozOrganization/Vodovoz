using System;

namespace Vodovoz.Core.Domain.Pacs
{
	public class OperatorWorkshift
	{
		public OperatorWorkshift()
		{
		}

		private OperatorWorkshift(int operatorId, DateTime startedAt, WorkShift workShift)
		{
			OperatorId = operatorId;
			Started = startedAt;
			PlannedWorkShift = workShift;
		}

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

		public static OperatorWorkshift Create(int operatorId, DateTime startedAt, WorkShift workShift)
		{
			if(workShift == null)
			{
				throw new ArgumentException("Ошибка создания смены оператора, нельзя создать смену оператора с пустой сменой", nameof(workShift));
			}

			return new OperatorWorkshift(operatorId, startedAt, workShift);
		}
	}
}
