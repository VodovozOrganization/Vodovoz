using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка фискального документа задачи отправки чека
	/// </summary>
	internal class FiscalDocumentRow
	{
		/// <summary>
		/// Идентификатор фискального документа
		/// </summary>
		public int FiscalDocumentId { get; set; }

		/// <summary>
		/// Идентификатор задачи отправки чека, по которой создан документ
		/// </summary>
		public int TaskId { get; set; }

		/// <summary>
		/// Время создания фискального документа
		/// </summary>
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// Время последнего изменения статуса.
		/// Пустое, если статус ни разу не менялся с момента создания
		/// </summary>
		public DateTime? StatusChangeTime { get; set; }

		/// <summary>
		/// Стадия фискального документа
		/// </summary>
		public FiscalDocumentStage Stage { get; set; }

		/// <summary>
		/// Статус фискального документа
		/// </summary>
		public FiscalDocumentStatus Status { get; set; }

		/// <summary>
		/// Фискальный номер документа
		/// </summary>
		public string FiscalNumber { get; set; }
	}
}
