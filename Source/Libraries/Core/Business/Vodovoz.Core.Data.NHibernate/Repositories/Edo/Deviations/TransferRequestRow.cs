using System;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка заявки на трансфер незавершенной итерации.
	/// Трансферная задача может быть еще не подобрана, поэтому здесь ее нет:
	/// соединение с ней выкинуло бы из выборки как раз те заявки,
	/// по которым трансфер так и не запустился
	/// </summary>
	internal class TransferRequestRow
	{
		/// <summary>
		/// Код заявки на трансфер
		/// </summary>
		public int RequestId { get; set; }

		/// <summary>
		/// Код задачи заказа, из-за которой понадобился трансфер
		/// </summary>
		public int OrderTaskId { get; set; }

		/// <summary>
		/// Время создания итерации трансфера
		/// </summary>
		public DateTime IterationTime { get; set; }
	}
}
