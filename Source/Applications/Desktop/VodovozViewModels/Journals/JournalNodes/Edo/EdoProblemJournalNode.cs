using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Edo
{
	/// <summary>
	/// Строка журнала проблем документооборота с клиентами
	/// </summary>
	public class EdoProblemJournalNode
	{
		/// <summary>
		/// Заказ
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Задача
		/// </summary>
		public int OrderTaskId { get; set; }

		/// <summary>
		/// Идентификатор источника проблемы
		/// </summary>
		public string ProblemSourceName { get; set; }

		/// <summary>
		/// Сообщение проблемы
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Дополнительная информация по проблеме
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Рекомендация информация по проблеме
		/// </summary>
		public string Recomendation { get; set; }

		/// <summary>
		/// Дата доставки заказа
		/// </summary>
		public DateTime DeliveryDate { get; set; }

		/// <summary>
		/// Статус задачи
		/// </summary>
		public EdoTaskStatus OrderTaskStatus { get; set; }

		/// <summary>
		/// Статус проблемы
		/// </summary>
		public TaskProblemState TaskProblemState { get; set; }
	}
}
