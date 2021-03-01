using Gamma.Widgets.Additions;
using QS.Views.GtkUI;
using Vodovoz.CommonEnums;
using Vodovoz.Domain.Logistic;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class CarFilterView : FilterViewBase<CarJournalFilterViewModel>
    {
        public CarFilterView(CarJournalFilterViewModel carJournalFilterViewModel) : base(carJournalFilterViewModel)
        {
            this.Build();
            Configure();
        }

        private void Configure()
        {
            ycheckIncludeArchive.Binding.AddBinding(ViewModel, vm => vm.IncludeArchive, w => w.Active)
                .InitializeFromSource();
            ycheckIncludeArchive.Binding.AddBinding(ViewModel, vm => vm.CanChangeIsArchive, w => w.Sensitive)
                .InitializeFromSource();

            yenumcomboRaskat.ItemsEnum = typeof(AllYesNo);
            yenumcomboRaskat.Binding.AddBinding(ViewModel, vm => vm.Raskat, w => w.SelectedItem).InitializeFromSource();
            yenumcomboRaskat.Binding.AddBinding(ViewModel, vm => vm.CanChangeRaskat, w => w.Sensitive)
                .InitializeFromSource();

            yenumcomboVisitingMaster.ItemsEnum = typeof(AllYesNo);
            yenumcomboVisitingMaster.Binding.AddBinding(ViewModel, vm => vm.VisitingMasters, w => w.SelectedItem)
                .InitializeFromSource();
            yenumcomboVisitingMaster.Binding.AddBinding(ViewModel, vm => vm.CanChangeVisitingMasters, w => w.Sensitive)
                .InitializeFromSource();

            enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
            enumcheckCarTypeOfUse.Binding
                .AddBinding(ViewModel, vm => vm.CanChangeRestrictedCarTypesOfUse, w => w.Sensitive)
                .InitializeFromSource();

            enumcheckCarTypeOfUse.Binding.AddBinding(ViewModel, vm => vm.RestrictedCarTypesOfUse,
                w => w.SelectedValuesList,
                new EnumsListConverter<CarTypeOfUse>()).InitializeFromSource();
        }
    }
}