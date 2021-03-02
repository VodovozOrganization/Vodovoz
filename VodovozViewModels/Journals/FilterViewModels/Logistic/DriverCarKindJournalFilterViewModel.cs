using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
    public class DriverCarKindJournalFilterViewModel : FilterViewModelBase<DriverCarKindJournalFilterViewModel>
    {
        private bool includeArchive;
        public virtual bool IncludeArchive {
            get => includeArchive;
            set => UpdateFilterField(ref includeArchive, value);
        }
    }
}
