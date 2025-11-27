using Vodovoz.Core.Domain.Edo;

namespace Edo.Scheduler
{
	/// <summary>
	/// Планировщик задач ЭДО для ручных заявок
	/// </summary>
	public class ManualTaskScheduler
	{
		/// <summary>
		/// Создаёт задачу ЭДО для ручной заявки
		/// </summary>
		/// <param name="edoRequest"></param>
		/// <returns></returns>
		public EdoTask CreateTask(ManualEdoRequest edoRequest)
		{
			var task = new ManualEdoTask
			{
				Status = EdoTaskStatus.New

			};

			edoRequest.Task = task;

			return task;
		}
	}
}
