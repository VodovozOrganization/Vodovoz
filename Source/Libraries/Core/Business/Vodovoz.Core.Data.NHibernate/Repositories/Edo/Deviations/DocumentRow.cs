using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка исходящего документа ЭДО
	/// </summary>
	internal class DocumentRow
	{
		/// <summary>
		/// Идентификатор исходящего документа
		/// </summary>
		public int DocumentId { get; set; }

		/// <summary>
		/// Идентификатор задачи ЭДО, по которой создан документ
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
