using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка задачи переноса кодов по заявке на трансфер
	/// </summary>
	internal class TransferTaskRow
	{
		/// <summary>
		/// Идентификатор заявки на трансфер, по которой подобрана задача
		/// </summary>
		public int RequestId { get; set; }

		/// <summary>
		/// Статус задачи трансфера
		/// </summary>
		public EdoTaskStatus Status { get; set; }

		/// <summary>
		/// Время начала переноса кодов.
		/// Пустое, пока перенос не запущен
		/// </summary>
		public DateTime? TransferStartTime { get; set; }
	}
}
