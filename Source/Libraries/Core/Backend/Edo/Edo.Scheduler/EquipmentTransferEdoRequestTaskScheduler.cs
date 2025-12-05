using Vodovoz.Core.Domain.Edo;

namespace Edo.Scheduler
{
	/// <summary>
	/// Планировщик задач ЭДО для заявок на акт приёма-передачи оборудования
	/// </summary>
	public class EquipmentTransferEdoRequestTaskScheduler
	{
		/// <summary>
		/// Создаёт задачу ЭДО для заявки на акт приёма-передачи оборудования
		/// </summary>
		/// <param name="edoRequest"></param>
		/// <returns></returns>
		public EdoTask CreateTask(EquipmentTransferEdoRequest edoRequest)
		{
			var task = new OrderDocumentEdoTask
			{
				Status = EdoTaskStatus.New

			};

			edoRequest.Task = task;

			return task;
		}
	}
}
