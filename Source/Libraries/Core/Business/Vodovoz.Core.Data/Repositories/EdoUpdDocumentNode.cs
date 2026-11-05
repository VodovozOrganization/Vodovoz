using System;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>Реквизиты сохранённого УПД и связанной задачи ЭДО.</summary>
	public class EdoUpdDocumentNode
	{
		/// <summary>Код УПД.</summary>
		public int Id { get; set; }
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
	}
}
