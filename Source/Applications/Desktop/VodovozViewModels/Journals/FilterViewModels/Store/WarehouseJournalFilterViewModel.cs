using System.Collections.Generic;
using QS.Project.Filter;
using QS.Project.Journal;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarehouseJournalFilterViewModel : FilterViewModelBase<WarehouseJournalFilterViewModel>, IJournalFilterViewModel 
	{
		public IEnumerable<int> IncludeWarehouseIds { get; set; }
		public int[] ExcludeWarehousesIds { get; set; }
		public bool IsShow { get; set; }
	}
}
