using System;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка заявки на трансфер незавершенной итерации
	/// </summary>
	internal class TransferRequestRow
	{
		/// <summary>
		/// Идентификатор заявки на трансфер
		/// </summary>
		public int RequestId { get; set; }

		/// <summary>
		/// Идентификатор задачи заказа, из-за которой понадобился трансфер
		/// </summary>
		public int OrderTaskId { get; set; }

		/// <summary>
		/// Время создания итерации трансфера
		/// </summary>
		public DateTime IterationTime { get; set; }
	}
}
