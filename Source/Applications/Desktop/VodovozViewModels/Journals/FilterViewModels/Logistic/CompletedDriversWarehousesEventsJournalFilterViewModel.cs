using System;
using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class CompletedDriversWarehousesEventsJournalFilterViewModel :
		FilterViewModelBase<CompletedDriversWarehousesEventsJournalFilterViewModel>
	{
		public CompletedDriversWarehousesEventsJournalFilterViewModel(
			Action<CompletedDriversWarehousesEventsJournalFilterViewModel> filterParameters = null)
		{
			if(filterParameters != null)
			{
				SetAndRefilterAtOnce(filterParameters);
			}
		}
		
		
	}
}
