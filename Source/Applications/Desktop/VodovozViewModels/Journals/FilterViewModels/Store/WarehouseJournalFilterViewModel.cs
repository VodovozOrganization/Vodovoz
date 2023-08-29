using System.Collections.Generic;
using QS.Project.Filter;
using QS.Project.Journal;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarehouseJournalFilterViewModel : FilterViewModelBase<WarehouseJournalFilterViewModel> 
	{
		public IEnumerable<int> IncludeWarehouseIds { get; set; }
		public int[] ExcludeWarehousesIds { get; set; }
		public override bool IsShow { get; set; } = true;
	}
}
