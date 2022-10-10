using QS.Project.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Complaints
{
	public class DriverComplaintReasonJournalFilterViewModel : FilterViewModelBase<DriverComplaintReasonJournalFilterViewModel>
	{
		private bool _isPopular;

		public bool IsPopular
		{ 
			get => _isPopular; 
			set => UpdateFilterField(ref _isPopular, value);
		}
	}
}
