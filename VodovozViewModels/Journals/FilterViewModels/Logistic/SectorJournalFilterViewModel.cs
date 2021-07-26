using QS.Project.Filter;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Journals.FilterViewModels
{
    public class SectorJournalFilterViewModel : FilterViewModelBase<SectorJournalFilterViewModel>
    {
        public SectorJournalFilterViewModel()
        {
            OnlyWithBorders = false;
        }
        
        private SectorsSetStatus? status;
        public SectorsSetStatus? Status {
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