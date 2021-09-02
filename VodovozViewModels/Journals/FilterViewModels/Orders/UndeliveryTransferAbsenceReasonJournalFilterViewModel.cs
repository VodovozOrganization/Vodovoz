using QS.Project.Filter;
using System;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Orders
{
	public class UndeliveryTransferAbsenceReasonJournalFilterViewModel : FilterViewModelBase<UndeliveryTransferAbsenceReasonJournalFilterViewModel>
	{
		private DateTime? _createEventDateFrom;
		private DateTime? _createEventDateEndTo;
		private bool _showArchive;

		public DateTime? CreateEventDateFrom
		{
			get => _createEventDateFrom;
			set => UpdateFilterField(ref _createEventDateFrom, value);
		}

		public DateTime? CreateEventDateTo
		{
			get => _createEventDateEndTo;
			set => UpdateFilterField(ref _createEventDateEndTo, value);
		}
		public bool ShowArchive
		{
			get => _showArchive;
			set => UpdateFilterField(ref _showArchive, value);
		}
	}
}
