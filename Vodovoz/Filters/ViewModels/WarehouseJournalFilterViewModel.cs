using System;
using QS.Project.Filter;
using QS.Project.Journal;

namespace Vodovoz.FilterViewModels.Warehouses
{
	public class WarehouseJournalFilterViewModel : FilterViewModelBase<WarehouseJournalFilterViewModel>
	{
		public WarehouseJournalFilterViewModel()
		{
			ResctrictArchive = true;
		}

		private bool restrictArchive;
		public virtual bool ResctrictArchive {
			get => restrictArchive;
			set => UpdateFilterField(ref restrictArchive, value, () => ResctrictArchive);
		}
	}
}
