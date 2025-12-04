using System;
using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Payments
{
	public class NotAllocatedCounterpartiesJournalFilterViewModel : FilterViewModelBase<NotAllocatedCounterpartiesJournalFilterViewModel>
	{
		private bool _showArchive;

		public bool ShowArchive
		{
			get => _showArchive;
			set => UpdateFilterField(ref _showArchive, value);
		}
	}
}
