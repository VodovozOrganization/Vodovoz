using System.Collections.Generic;
using QS.Project.Filter;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;

namespace Vodovoz.ViewModels.Journals.FilterViewModels
{
    public class ExpenseCategoryJournalFilterViewModel: FilterViewModelBase<ExpenseCategoryJournalFilterViewModel>
    {
        private bool showArchive;
        public bool ShowArchive {
            get => showArchive;
            set => UpdateFilterField(ref showArchive, value);
        }

		private bool _onlyWithoutNewCategoryLink;
		public bool OnlyWithoutNewCategoryLink
		{
			get => _onlyWithoutNewCategoryLink;
			set => UpdateFilterField(ref _onlyWithoutNewCategoryLink, value);
		}
        
        private LevelsFilter? level = LevelsFilter.All;
        public LevelsFilter? Level
        {
            get => level;
            set => UpdateFilterField(ref level, value);
        }
        
        private IEnumerable<int> excludedIds;
        public virtual IEnumerable<int> ExcludedIds {
            get => excludedIds;
            set => UpdateFilterField(ref excludedIds, value);
        }
	}
}
