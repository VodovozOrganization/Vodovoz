using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Order;

namespace Vodovoz.Filters.GtkViews
{
    public partial class NomenclaturePlanFilterView : FilterViewBase<NomenclaturePlanFilterViewModel>
    {
        public NomenclaturePlanFilterView(NomenclaturePlanFilterViewModel filterViewModel) : base(filterViewModel)
        {
            this.Build();
            filterViewModel.NomenclatureFilterViewModel.RefreshFilter();
            ychkOnlyPlanned.Binding.AddBinding(ViewModel, vm => vm.IsOnlyPlanned, w => w.Active).InitializeFromSource();
        }
    }
}
