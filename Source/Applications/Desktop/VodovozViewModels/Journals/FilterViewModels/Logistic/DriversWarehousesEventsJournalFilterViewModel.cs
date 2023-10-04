using System;
using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class DriversWarehousesEventsJournalFilterViewModel : FilterViewModelBase<DriversWarehousesEventsJournalFilterViewModel>
	{
		public DriversWarehousesEventsJournalFilterViewModel(
			Action<DriversWarehousesEventsJournalFilterViewModel> filterParameters = null)
		{
			if(filterParameters != null)
			{
				SetAndRefilterAtOnce(filterParameters);
			}
		}
		
		
	}
}
