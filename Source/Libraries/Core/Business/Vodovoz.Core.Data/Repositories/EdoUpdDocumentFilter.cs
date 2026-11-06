using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>Условия выборки сохранённых УПД.</summary>
	public class EdoUpdDocumentFilter
	{
		/// <summary>Включительная нижняя граница даты документа.</summary>
		public DateTime? DateFrom { get; set; }
		/// <summary>Исключительная верхняя граница даты документа.</summary>
		public DateTime? DateToExclusive { get; set; }
		/// <summary>Тип задачи, связанной с УПД.</summary>
		public EdoTaskType? TaskType { get; set; }
		/// <summary>ИНН покупателя; null отключает ограничение.</summary>
		public string CustomerInn { get; set; }
	}
}
