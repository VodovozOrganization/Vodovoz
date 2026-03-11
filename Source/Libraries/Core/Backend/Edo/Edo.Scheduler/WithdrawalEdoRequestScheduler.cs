using System;
using QS.Extensions.Observable.Collections.List;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

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

			var baseDocumentTask = withdrawalRequest.BaseDocumentEdoTask;

			var codes = baseDocumentTask?.Items?.Select(x => x.ProductCode) ?? Enumerable.Empty<TrueMarkProductCode>();

			task.Items = new ObservableList<EdoTaskItem>(
				codes.Select(x =>
					new EdoTaskItem
					{
						ProductCode = x,
						CustomerEdoTask = task
					})
			);

			withdrawalRequest.Task = task;

			return task;
		}
	}
}
