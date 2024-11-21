using QS.Project.Filter;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class CarEventTypeFilterViewModel : FilterViewModelBase<CarEventTypeFilterViewModel>
	{
		public List<int> ExcludeCarEventTypeIds { get; } = new List<int>();
	}
}
