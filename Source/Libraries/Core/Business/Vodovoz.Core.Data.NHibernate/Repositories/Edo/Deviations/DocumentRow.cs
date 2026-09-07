using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка исходящего документа ЭДО.
	/// Одним типом описываются и документы заказов, и документы трансфера:
	/// дальше документооборот у них общий
	/// </summary>
	internal class DocumentRow
	{
		/// <summary>
		/// Код исходящего документа
		/// </summary>
		public int DocumentId { get; set; }

		/// <summary>
		/// Код задачи ЭДО, по которой создан документ
		/// </summary>
		public int TaskId { get; set; }

		/// <summary>
		/// Время создания документа
		/// </summary>
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// Статус документа
		/// </summary>
		public EdoDocumentStatus Status { get; set; }
	}
}
