using System;
using QS.Project.Filter;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Edo
{
	/// <summary>
	/// Фильтр журнала УПД по реквизитам сохранённого документа.
	/// </summary>
	public class EdoUpdFilterViewModel : FilterViewModelBase<EdoUpdFilterViewModel>
	{
		private DateTime? _dateFrom;
		private DateTime? _dateTo;
		private EdoUpdDocumentType? _documentType;
		private string _customerInn;

		/// <summary>Начальная дата УПД.</summary>
		public DateTime? DateFrom
		{
			get => _dateFrom;
			set => UpdateFilterField(ref _dateFrom, value);
		}
		/// <summary>Конечная дата УПД включительно.</summary>
		public DateTime? DateTo
		{
			get => _dateTo;
			set => UpdateFilterField(ref _dateTo, value);
		}
		/// <summary>Тип документа.</summary>
		public EdoUpdDocumentType? DocumentType
		{
			get => _documentType;
			set => UpdateFilterField(ref _documentType, value);
		}
		/// <summary>ИНН покупателя.</summary>
		public string CustomerInn
		{
			get => _customerInn;
			set => UpdateFilterField(ref _customerInn, value);
		}
	}
}
