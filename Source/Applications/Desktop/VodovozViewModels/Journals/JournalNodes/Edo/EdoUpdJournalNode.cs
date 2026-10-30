using System;
using System.ComponentModel.DataAnnotations;
using QS.Project.Journal;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Edo
{
	/// <summary>
	/// Источник документа УПД.
	/// </summary>
	public enum EdoUpdDocumentType
	{
		/// <summary>УПД клиентского заказа.</summary>
		[Display(Name = "Заказ")]
		Order,
		/// <summary>УПД трансфера между организациями.</summary>
		[Display(Name = "Трансфер")]
		Transfer
	}

	/// <summary>
	/// Строка журнала сохранённых УПД.
	/// </summary>
	public class EdoUpdJournalNode : JournalNodeBase
	{
		/// <summary>Код сохранённого УПД.</summary>
		public int Id { get; set; }
		/// <summary>Название документа.</summary>
		public override string Title => $"УПД №{Number} от {Date:dd.MM.yyyy}";
		/// <summary>Номер УПД.</summary>
		public string Number { get; set; }
		/// <summary>Дата УПД.</summary>
		public DateTime Date { get; set; }
		/// <summary>Сумма УПД.</summary>
		public decimal Sum { get; set; }
		/// <summary>Продавец.</summary>
		public string Seller { get; set; }
		/// <summary>Покупатель.</summary>
		public string Customer { get; set; }
		/// <summary>ИНН покупателя из сохранённого УПД.</summary>
		public string CustomerInn { get; set; }
		/// <summary>Тип задачи ЭДО.</summary>
		public string TaskType { get; set; }
		/// <summary>Код задачи ЭДО.</summary>
		public int TaskId { get; set; }
		/// <summary>Тип документа в журнале.</summary>
		public EdoUpdDocumentType DocumentType => TaskType == "Transfer" ? EdoUpdDocumentType.Transfer : EdoUpdDocumentType.Order;
	}
}
