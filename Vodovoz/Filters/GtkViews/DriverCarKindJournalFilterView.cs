using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.Filters.GtkViews
{
    public partial class DriverCarKindJournalFilterView : FilterViewBase<DriverCarKindJournalFilterViewModel>
    {
        public DriverCarKindJournalFilterView(DriverCarKindJournalFilterViewModel filterViewModel) : base(filterViewModel)
        {
            this.Build();
            ycheckIncludeArchive.Binding.AddBinding(ViewModel, vm => vm.IncludeArchive, w => w.Active).InitializeFromSource();
        }
    }
}
