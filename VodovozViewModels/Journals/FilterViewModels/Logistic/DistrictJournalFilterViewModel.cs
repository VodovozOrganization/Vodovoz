using QS.Project.Filter;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Journals.FilterViewModels
{
    public class DistrictJournalFilterViewModel : FilterViewModelBase<DistrictJournalFilterViewModel>
    {
        public DistrictJournalFilterViewModel()
        {
            OnlyWithBorders = false;
        }
        
        private DistrictsSetStatus? status;
        public DistrictsSetStatus? Status {
            get => status;
            set => UpdateFilterField(ref status, value, () => Status);
        }
        
        private bool onlyWithBorders;
        public bool OnlyWithBorders {
            get => onlyWithBorders;
            set => UpdateFilterField(ref onlyWithBorders, value, () => OnlyWithBorders);
        }
    }
}