using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Отметки времени последних обработанных документов Такскома
	/// </summary>
	public class TaxcomEdoDocflowLastProcessTime : IDomainObject
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Отметка последнего обработанного события исходящих документов(сохраняется дата изменения статуса из Такском)
		/// </summary>
		public virtual DateTime LastProcessedEventOutgoingDocuments { get; set; }

		/// <summary>
		/// Отметка последнего обработанного события входящих документов(сохраняется дата изменения статуса из Такском)
		/// </summary>
		public virtual DateTime LastProcessedEventIngoingDocuments { get; set; }

		/// <summary>
		/// Отметка последнего обработанного события документов
		/// ожидающих аннулирования(сохраняется дата изменения статуса из Такском)
		/// </summary>
		public virtual DateTime LastProcessedEventWaitingForCancellationDocuments { get; set; }

		/// <summary>
		/// Id организации
		/// </summary>
		public virtual int OrganizationId { get; set; }
	}
}
