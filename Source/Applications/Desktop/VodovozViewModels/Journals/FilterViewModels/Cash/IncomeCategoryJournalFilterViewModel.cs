using System.ComponentModel.DataAnnotations;
using QS.Project.Filter;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;

namespace Vodovoz.ViewModels.Journals.FilterViewModels
{
    public class IncomeCategoryJournalFilterViewModel: FilterViewModelBase<IncomeCategoryJournalFilterViewModel>
    {
        private bool showArchive;
        public bool ShowArchive {
            get => showArchive;
            set => UpdateFilterField(ref showArchive, value);
        }

        private LevelsFilter? level = LevelsFilter.All;
        public LevelsFilter? Level
        {
            get => level;
            set => UpdateFilterField(ref level, value);
        }
        
    }
}