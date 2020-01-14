using System;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.Services;

namespace Vodovoz.Filters.ViewModels
{
	public class CarJournalFilterViewModel : FilterViewModelBase<CarJournalFilterViewModel>, IJournalFilter
	{
		public CarJournalFilterViewModel()
		{
			UpdateWith(
				x => x.IncludeArchive
			);
		}

		private bool includeArchive;
		public virtual bool IncludeArchive {
			get => includeArchive;
			set => SetField(ref includeArchive, value, () => IncludeArchive);
		}
	}
}
