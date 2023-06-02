using Gamma.Widgets.Additions;
using QS.Views.GtkUI;
using QS.Widgets;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.Filters.GtkViews
{
	public partial class CarFilterView : FilterViewBase<CarJournalFilterViewModel>
	{
		public CarFilterView(CarJournalFilterViewModel carJournalFilterViewModel) : base(carJournalFilterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			nullablecheckArchive.RenderMode = RenderMode.Icon;
			nullablecheckArchive.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeIsArchive, w => w.Sensitive)
				.AddBinding(vm => vm.Archive, w => w.Active)
				.InitializeFromSource();

			nullablecheckVisitingMaster.RenderMode = RenderMode.Icon;
			nullablecheckVisitingMaster.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeVisitingMasters, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.VisitingMasters, w => w.Active)
				.InitializeFromSource();

			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeRestrictedCarTypesOfUse, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictedCarTypesOfUse, w => w.SelectedValuesList, new EnumsListConverter<CarTypeOfUse>())
				.InitializeFromSource();

			enumcheckCarOwnType.EnumType = typeof(CarOwnType);
			enumcheckCarOwnType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeRestrictedCarOwnTypes, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictedCarOwnTypes, w => w.SelectedValuesList, new EnumsListConverter<CarOwnType>())
				.InitializeFromSource();

			entryModel.SetEntityAutocompleteSelectorFactory(ViewModel.CarModelJournalFactory.CreateCarModelAutocompleteSelectorFactory());
			entryModel.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeCarModel, w => w.Sensitive)
				.AddBinding(vm => vm.CarModel, w => w.Subject)
				.InitializeFromSource();
		}
	}
}
