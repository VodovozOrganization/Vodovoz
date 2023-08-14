using System;
using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Orders
{
	public class UndeliveryObjectJournalFilterViewModel : FilterViewModelBase<UndeliveryObjectJournalFilterViewModel>
	{
		private bool _isArchive;

		public bool IsArchive
		{
			get => _isArchive;
			set => UpdateFilterField(ref _isArchive, value);
		}
	}
}
