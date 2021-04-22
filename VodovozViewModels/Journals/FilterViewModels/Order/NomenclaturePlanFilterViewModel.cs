using System;
using QS.Project.Filter;
using Vodovoz.Domain.Goods;
using Vodovoz.FilterViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Order
{
    public class NomenclaturePlanFilterViewModel : FilterViewModelBase<NomenclaturePlanFilterViewModel>
    {
        public NomenclatureFilterViewModel NomenclatureFilterViewModel { get; }

        public NomenclaturePlanFilterViewModel(NomenclatureFilterViewModel nomenclatureFilterViewModel)
        {
            NomenclatureFilterViewModel = nomenclatureFilterViewModel;
            NomenclatureFilterViewModel.OnFiltered += NomenclatureFilterViewModel_OnFiltered;
        }

        private void NomenclatureFilterViewModel_OnFiltered(object sender, EventArgs e) => Update();

        private bool isOnlyPlanned;
        public virtual bool IsOnlyPlanned
        {
            get => isOnlyPlanned;
            set => UpdateFilterField(ref isOnlyPlanned, value);
        }
    }
}
