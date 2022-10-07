using QS.Project.Filter;
using System;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Complaints
{
	public class ComplaintObjectJournalFilterViewModel : FilterViewModelBase<ComplaintObjectJournalFilterViewModel>
	{
		private DateTime? _createDateFrom;
		private DateTime? _createDateTo;
		private bool _isArchive;

		public DateTime? CreateDateFrom
		{
			get => _createDateFrom;
			set => UpdateFilterField(ref _createDateFrom, value);
		}

		public DateTime? CreateDateTo
		{
			get => _createDateTo;
			set => UpdateFilterField(ref _createDateTo, value);
		}

		public bool IsArchive
		{
			get => _isArchive;
			set => UpdateFilterField(ref _isArchive, value);
		}
	}
}
