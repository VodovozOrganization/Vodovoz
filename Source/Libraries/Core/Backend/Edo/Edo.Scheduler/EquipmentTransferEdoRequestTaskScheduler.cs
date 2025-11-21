using Vodovoz.Core.Domain.Edo;

namespace Edo.Scheduler
{
	public class EquipmentTransferEdoRequestTaskScheduler
	{
		public EdoTask CreateTask(EquipmentTransferEdoRequest edoRequest)
		{
			var task = new EquipmentTransferEdoTask
			{
				Status = EdoTaskStatus.New
			};

			edoRequest.Task = task;

			return task;
		}
	}
}
