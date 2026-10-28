using System;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;

namespace Vodovoz.ViewModels.Services.Edo
{
	/// <summary>Условия выборки сохранённых УПД.</summary>
	public class EdoUpdJournalFilter
	{
		/// <summary>Начальная дата УПД.</summary>
		public DateTime? DateFrom { get; set; }
		/// <summary>Конечная дата УПД включительно.</summary>
		public DateTime? DateTo { get; set; }
		/// <summary>Тип документа.</summary>
		public EdoUpdDocumentType? DocumentType { get; set; }
		/// <summary>ИНН покупателя из сохранённого УПД.</summary>
		public string CustomerInn { get; set; }
	}
}
