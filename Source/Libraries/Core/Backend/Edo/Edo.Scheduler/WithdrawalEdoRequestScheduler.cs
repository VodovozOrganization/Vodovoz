using System;
using QS.Extensions.Observable.Collections.List;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Scheduler.Service
{
	/// <summary>
	/// Планировщик задач для заявок на вывод кодов из оборота
	/// </summary>
	public class WithdrawalEdoRequestScheduler
	{
		/// <summary>
		/// Создать задачу на вывод из оборота из заявки
		/// </summary>
		/// <param name="withdrawalRequest">Заявка на вывод из оборота</param>
		/// <returns>Задача на вывод из оборота</returns>
		public WithdrawalEdoTask CreateTask(WithdrawalEdoRequest withdrawalRequest)
		{
			if(withdrawalRequest == null)
			{
				throw new ArgumentNullException(nameof(withdrawalRequest));
			}

			var task = new WithdrawalEdoTask
			{
				Status = EdoTaskStatus.New,
			};

			if(withdrawalRequest.ProductCodes != null && withdrawalRequest.ProductCodes.Any())
			{
				task.Items = new ObservableList<EdoTaskItem>(
					withdrawalRequest.ProductCodes.Select(x =>
						new EdoTaskItem
						{
							ProductCode = x,
							CustomerEdoTask = task
						})
				);
			}

			withdrawalRequest.Task = task;

			return task;
		}
	}
}
